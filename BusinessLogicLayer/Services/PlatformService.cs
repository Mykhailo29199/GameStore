using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class PlatformService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IPlatformService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreatePlatformAsync(string platformType)
    {
        var existType = (await _unitOfWork.PlatformRepository
        .FindAsync(p => p.Type == platformType))
        .FirstOrDefault();

        if (existType is not null)
        {
            throw new ArgumentException($"Platform with type '{platformType}' already exists.");
        }

        var platformModel = new PlatformModel
        {
            Type = platformType,
        };

        var platformEntity = _mapper.Map<PlatformEntity>(platformModel);

        await _unitOfWork.PlatformRepository.AddAsync(platformEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<PlatformModel> GetPlatformByIdAsync(Guid platformId)
    {
        var platformEntity = await _unitOfWork.PlatformRepository.GetByIdAsync(platformId)
            ?? throw new NotFoundException($"Platform with id '{platformId}' does not exist.");

        var platformModel = _mapper.Map<PlatformModel>(platformEntity);

        return platformModel;
    }

    public async Task<List<PlatformModel>> GetAllPlatformsAsync()
    {
        var platformEntities = await _unitOfWork.PlatformRepository.GetAllAsync();

        var platformModels = _mapper.Map<List<PlatformModel>>(platformEntities);

        return platformModels;
    }

    public async Task UpdatePlatformAsync(PlatformModel platformModel)
    {
        var platformEntity = await _unitOfWork.PlatformRepository.GetByIdAsync(platformModel.Id)
            ?? throw new NotFoundException($"Platform with id '{platformModel.Id}' does not exist.");

        _mapper.Map(platformModel, platformEntity);

        await _unitOfWork.PlatformRepository.UpdateAsync(platformEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task DeletePlatformAsync(Guid platformId)
    {
        var platformEntity = await _unitOfWork.PlatformRepository.GetByIdAsync(platformId)
            ?? throw new NotFoundException($"Platform with id '{platformId}' does not exist.");
        await _unitOfWork.PlatformRepository.DeleteAsync(platformEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        _ = await _unitOfWork.PlatformRepository.GetByIdAsync(platformId)
            ?? throw new NotFoundException($"Platform with id '{platformId}' does not exist.");

        var gameEntities = await _unitOfWork.GamePlatformRepository.GetGamesByPlatformIdAsync(platformId);

        var gameModels = _mapper.Map<List<GameModel>>(gameEntities);

        return gameModels;
    }
}
