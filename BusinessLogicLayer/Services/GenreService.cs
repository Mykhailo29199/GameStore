using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BusinessLogicLayer.Catalog;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class GenreService(
    ISqlServerUnitOfWork unitOfWork,
    IMapper mapper,
    ICategoryCatalog categoryCatalog,
    IProductCatalog productCatalog) : IGenreService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreateGenreAsync(GenreModel genreModel)
    {
        if (genreModel.ParentGenreId.HasValue)
        {
            _ = await categoryCatalog.GetByIdAsync(genreModel.ParentGenreId.Value)
                ?? throw new NotFoundException($"Parent genre with Id {genreModel.ParentGenreId} does not exist.");
        }

        if (genreModel.ParentGenreId.HasValue && genreModel.ParentGenreId.Value == genreModel.Id)
        {
            throw new ValidationException("A genre cannot be its own parent.");
        }

        if (genreModel.ParentGenreId.HasValue
            && await IsCyclicReferenceAsync(genreModel.ParentGenreId.Value, genreModel.Id))
        {
            throw new ValidationException("Cyclic genre references are not allowed.");
        }

        await EnsureNameIsUniqueAsync(genreModel);

        if (genreModel.ParentGenreId.HasValue)
        {
            await categoryCatalog.EnsureInGameStoreAsync(genreModel.ParentGenreId.Value);
        }

        var genreEntity = _mapper.Map<CategoryEntity>(genreModel);
        await _unitOfWork.GenreRepository.AddAsync(genreEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<GenreModel> GetGenreByIdAsync(Guid genreId)
    {
        var genreEntity = await categoryCatalog.GetByIdAsync(genreId)
            ?? throw new NotFoundException($"Genre with Id {genreId} does not exist.");

        var genreModel = _mapper.Map<GenreModel>(genreEntity);
        return genreModel;
    }

    public async Task<List<GenreModel>> GetAllGenresAsync()
    {
        var genreEntities = await categoryCatalog.GetAllAsync();
        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        return genreModels;
    }

    public async Task<IList<GenreModel>> GetGenresByParentGenreIdAsync(Guid parentGenreId)
    {
        _ = await categoryCatalog.GetByIdAsync(parentGenreId)
            ?? throw new NotFoundException($"Parent genre with Id {parentGenreId} does not exist.");

        var genreEntities = await _unitOfWork.GenreRepository
            .FindAsync(g => g.ParentCategoryId == parentGenreId);

        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        return genreModels;
    }

    /// <summary>
    /// A Northwind genre is copied to the Game Store database and the copy is updated; Northwind data stays untouched.
    /// </summary>
    public async Task UpdateGenreAsync(GenreModel genreModel)
    {
        _ = await categoryCatalog.GetByIdAsync(genreModel.Id)
            ?? throw new NotFoundException($"Genre with Id {genreModel.Id} does not exist.");

        if (genreModel.ParentGenreId.HasValue)
        {
            _ = await categoryCatalog.GetByIdAsync(genreModel.ParentGenreId.Value)
                ?? throw new NotFoundException($"Parent genre with Id {genreModel.ParentGenreId} does not exist.");
        }

        if (genreModel.ParentGenreId == genreModel.Id)
        {
            throw new ValidationException("A genre cannot be its own parent.");
        }

        if (genreModel.ParentGenreId.HasValue
           && await IsCyclicReferenceAsync(genreModel.ParentGenreId.Value, genreModel.Id))
        {
            throw new ValidationException("Cyclic genre references are not allowed.");
        }

        await EnsureNameIsUniqueAsync(genreModel);

        var existingGenreEntity = await categoryCatalog.EnsureInGameStoreAsync(genreModel.Id);

        if (genreModel.ParentGenreId.HasValue)
        {
            await categoryCatalog.EnsureInGameStoreAsync(genreModel.ParentGenreId.Value);
        }

        existingGenreEntity.Name = genreModel.Name;
        existingGenreEntity.ParentCategoryId = genreModel.ParentGenreId;

        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Genres that exist in Northwind are soft-deleted in the Game Store database, so the Northwind original stays hidden.
    /// </summary>
    public async Task DeleteGenreAsync(Guid id)
    {
        var genre = await categoryCatalog.GetByIdAsync(id)
            ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

        if (!await categoryCatalog.HasNorthwindCounterpartAsync(genre))
        {
            var existingGenreEntity = await _unitOfWork.GenreRepository.GetWithSubGenresAsync(id)
                ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

            await _unitOfWork.GenreRepository.DeleteAsync(existingGenreEntity);
        }
        else
        {
            var gameStoreGenre = await categoryCatalog.EnsureInGameStoreAsync(id);
            gameStoreGenre.IsDeleted = true;
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByGenreIdAsync(Guid genreId)
    {
        _ = await categoryCatalog.GetByIdAsync(genreId)
          ?? throw new NotFoundException($"Genre with id '{genreId}' does not exist.");

        var gameEntities = (await productCatalog.GetAllAsync())
            .Where(g => g.ProductCategories.Any(pc => pc.CategoryId == genreId))
            .ToList();

        var gameModels = _mapper.Map<List<GameModel>>(gameEntities);

        return gameModels;
    }

    private async Task EnsureNameIsUniqueAsync(GenreModel genreModel)
    {
        var name = NorthwindResolver.Normalize(genreModel.Name);
        var genres = await categoryCatalog.GetAllAsync();

        if (genres.Any(g => g.Id != genreModel.Id && NorthwindResolver.Normalize(g.Name) == name))
        {
            throw new BadRequestException($"Genre with name '{genreModel.Name}' already exists.");
        }
    }

    private async Task<bool> IsCyclicReferenceAsync(Guid parentGenreId, Guid genreId)
    {
        var currentParentId = parentGenreId;

        while (true)
        {
            var parent = await categoryCatalog.GetByIdAsync(currentParentId);
            if (parent == null)
            {
                break;
            }

            if (parent.Id == genreId)
            {
                return true; // Cyclic reference detected
            }

            if (!parent.ParentCategoryId.HasValue)
            {
                break;
            }

            currentParentId = parent.ParentCategoryId.Value;
        }

        return false;
    }
}
