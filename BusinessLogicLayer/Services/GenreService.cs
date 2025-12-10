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
                ?? throw new ArgumentException($"Parent genre with Id {genreModel.ParentGenreId} does not exist.");
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
        var parentGenreEntity = await _unitOfWork.GenreRepository.GetByIdAsync(parentGenreId)
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
                ?? throw new ArgumentException($"Parent genre with Id {genreModel.ParentGenreId} does not exist.");
        }

        _mapper.Map(genreModel, existingGenreEntity);
        await _unitOfWork.GenreRepository.UpdateAsync(existingGenreEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteGenreAsync(Guid id)
    {
        var existingGenreEntity = await _unitOfWork.GenreRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

        var subGenres = await _unitOfWork.GenreRepository.FindAsync(g => g.ParentGenreId == existingGenreEntity.Id);

        foreach (var sub in subGenres)
        {
            sub.ParentGenreId = null;
        }

        await _unitOfWork.GenreRepository.DeleteAsync(existingGenreEntity);
        await _unitOfWork.SaveAsync();
    }
}
