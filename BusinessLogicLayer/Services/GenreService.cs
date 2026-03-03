using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class GenreService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IGenreService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreateGenreAsync(GenreModel genreModel)
    {
        if (genreModel.ParentGenreId.HasValue)
        {
            _ = await _unitOfWork.GenreRepository
                .GetByIdAsync(genreModel.ParentGenreId.Value)
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

        var genreEntity = _mapper.Map<GenreEntity>(genreModel);
        await _unitOfWork.GenreRepository.AddAsync(genreEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<GenreModel> GetGenreByIdAsync(Guid genreId)
    {
        var genreEntity = await _unitOfWork.GenreRepository.GetByIdAsync(genreId)
            ?? throw new NotFoundException($"Genre with Id {genreId} does not exist.");

        var genreModel = _mapper.Map<GenreModel>(genreEntity);
        return genreModel;
    }

    public async Task<List<GenreModel>> GetAllGenresAsync()
    {
        var genreEntities = await _unitOfWork.GenreRepository.GetAllAsync();
        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        return genreModels;
    }

    public async Task<IList<GenreModel>> GetGenresByParentGenreIdAsync(Guid parentGenreId)
    {
        _ = await _unitOfWork.GenreRepository.GetByIdAsync(parentGenreId)
            ?? throw new NotFoundException($"Parent genre with Id {parentGenreId} does not exist.");

        var genreEntities = await _unitOfWork.GenreRepository
            .FindAsync(g => g.ParentGenreId == parentGenreId);

        var genreModels = _mapper.Map<List<GenreModel>>(genreEntities);
        return genreModels;
    }

    public async Task UpdateGenreAsync(GenreModel genreModel)
    {
        var existingGenreEntity = await _unitOfWork.GenreRepository.GetByIdAsync(genreModel.Id)
            ?? throw new NotFoundException($"Genre with Id {genreModel.Id} does not exist.");

        if (genreModel.ParentGenreId.HasValue)
        {
            _ = await _unitOfWork.GenreRepository
                .GetByIdAsync(genreModel.ParentGenreId.Value)
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

        _mapper.Map(genreModel, existingGenreEntity);
        await _unitOfWork.GenreRepository.UpdateAsync(existingGenreEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteGenreAsync(Guid id)
    {
        var existingGenreEntity = await _unitOfWork.GenreRepository.GetWithSubGenresAsync(id)
            ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

        await _unitOfWork.GenreRepository.DeleteAsync(existingGenreEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByGenreIdAsync(Guid genreId)
    {
        _ = await _unitOfWork.GenreRepository.GetByIdAsync(genreId)
          ?? throw new NotFoundException($"Genre with id '{genreId}' does not exist.");

        var gameEntities = await _unitOfWork.GameGenreRepository.GetGamesByGenreIdAsync(genreId);

        var gameModels = _mapper.Map<List<GameModel>>(gameEntities);

        return gameModels;
    }

    private async Task<bool> IsCyclicReferenceAsync(Guid parentGenreId, Guid genreId)
    {
        var currentParentId = parentGenreId;

        while (true)
        {
            var parent = await _unitOfWork.GenreRepository.GetByIdAsync(currentParentId);
            if (parent == null)
            {
                break;
            }

            if (parent.Id == genreId)
            {
                return true;
            }

            if (!parent.ParentGenreId.HasValue)
            {
                break;
            }

            currentParentId = parent.ParentGenreId.Value;
        }

        return false;
    }
}
