using AutoMapper;
using BusinessLogicLayer.Catalog;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Localization;
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
    IProductCatalog productCatalog,
    IContentLocalizer localizer) : IGenreService
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
            throw new BadRequestException("A genre cannot be its own parent.");
        }

        if (genreModel.ParentGenreId.HasValue
            && await IsCyclicReferenceAsync(genreModel.ParentGenreId.Value, genreModel.Id))
        {
            throw new BadRequestException("Cyclic genre references are not allowed.");
        }

        await EnsureNameIsUniqueAsync(genreModel);

        if (genreModel.ParentGenreId.HasValue)
        {
            await categoryCatalog.EnsureInGameStoreAsync(genreModel.ParentGenreId.Value);
        }

        var genreEntity = _mapper.Map<CategoryEntity>(genreModel);

        // English data is required, so a genre added in another language also gets it as the English version.
        if (!localizer.IsDefaultLanguage)
        {
            await localizer.SaveGenreTranslationAsync(genreEntity, genreModel.Name);
        }

        await _unitOfWork.CategoryRepository.AddAsync(genreEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<GenreModel> GetGenreByIdAsync(Guid genreId)
    {
        var genreEntity = await categoryCatalog.GetByIdAsync(genreId)
            ?? throw new NotFoundException($"Genre with Id {genreId} does not exist.");

        var genreModel = _mapper.Map<GenreModel>(genreEntity);
        await localizer.LocalizeGenresAsync([genreModel]);

        return genreModel;
    }

    public async Task<List<GenreModel>> GetAllGenresAsync()
    {
        var genreEntities = await categoryCatalog.GetAllAsync();
        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        await localizer.LocalizeGenresAsync(genreModels);

        return genreModels;
    }

    public async Task<IList<GenreModel>> GetGenresByParentGenreIdAsync(Guid parentGenreId)
    {
        _ = await categoryCatalog.GetByIdAsync(parentGenreId)
            ?? throw new NotFoundException($"Parent genre with Id {parentGenreId} does not exist.");

        var genreEntities = await _unitOfWork.CategoryRepository
            .FindAsync(g => g.ParentCategoryId == parentGenreId);

        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        await localizer.LocalizeGenresAsync(genreModels);

        return genreModels;
    }

    /// <summary>
    /// A Northwind genre is copied to the Game Store database and the copy is updated; Northwind data stays untouched.
    /// The name is saved in the language of the request.
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
            throw new BadRequestException("A genre cannot be its own parent.");
        }

        if (genreModel.ParentGenreId.HasValue
            && await IsCyclicReferenceAsync(genreModel.ParentGenreId.Value, genreModel.Id))
        {
            throw new BadRequestException("Cyclic genre references are not allowed.");
        }

        await EnsureNameIsUniqueAsync(genreModel);

        var existingGenreEntity = await categoryCatalog.EnsureInGameStoreAsync(genreModel.Id);

        if (genreModel.ParentGenreId.HasValue)
        {
            await categoryCatalog.EnsureInGameStoreAsync(genreModel.ParentGenreId.Value);
        }

        if (localizer.IsDefaultLanguage)
        {
            existingGenreEntity.Name = genreModel.Name;
        }
        else
        {
            await localizer.SaveGenreTranslationAsync(existingGenreEntity, genreModel.Name);
        }

        existingGenreEntity.ParentCategoryId = genreModel.ParentGenreId;

        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Genres that exist in Northwind are soft-deleted in the Game Store database, so the Northwind original stays hidden.
    /// All translations of the genre are removed in the same operation: by the database cascade or explicitly.
    /// </summary>
    public async Task DeleteGenreAsync(Guid id)
    {
        var genre = await categoryCatalog.GetByIdAsync(id)
            ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

        if (!await categoryCatalog.HasNorthwindCounterpartAsync(genre))
        {
            var existingGenreEntity = await _unitOfWork.CategoryRepository.GetWithSubGenresAsync(id)
                ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

            await _unitOfWork.CategoryRepository.DeleteAsync(existingGenreEntity);
        }
        else
        {
            var gameStoreGenre = await categoryCatalog.EnsureInGameStoreAsync(id);
            await localizer.RemoveTranslationsAsync(gameStoreGenre);
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
        await localizer.LocalizeGamesAsync(gameModels);

        return gameModels;
    }

    // Names are compared in the language of the request, as the user sees them.
    private async Task EnsureNameIsUniqueAsync(GenreModel genreModel)
    {
        var name = NorthwindResolver.Normalize(genreModel.Name);
        var genres = await GetAllGenresAsync();

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
