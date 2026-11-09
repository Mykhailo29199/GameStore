using AutoMapper;
using BusinessLogicLayer.Localization;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class PlatformService(ISqlServerUnitOfWork unitOfWork, IMapper mapper, IContentLocalizer localizer) : IPlatformService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreatePlatformAsync(string platformType)
    {
        var existType = (await _unitOfWork.PlatformRepository
        .FindAsync(p => p.Type == platformType))
        .FirstOrDefault();

        if (existType is not null)
        {
            throw new BadRequestException($"Platform with type '{platformType}' already exists.");
        }

        var platformModel = new PlatformModel
        {
            Type = platformType,
        };

        var platformEntity = _mapper.Map<PlatformEntity>(platformModel);

        // English data is required, so a platform added in another language also gets it as the English version.
        if (!localizer.IsDefaultLanguage)
        {
            await localizer.SavePlatformTranslationAsync(platformEntity, platformType);
        }

        await _unitOfWork.PlatformRepository.AddAsync(platformEntity);

        await _unitOfWork.SaveAsync();
    }

    public async Task<PlatformModel> GetPlatformByIdAsync(Guid platformId)
    {
        var platformEntity = await _unitOfWork.PlatformRepository.GetByIdAsync(platformId)
            ?? throw new NotFoundException($"Platform with id '{platformId}' does not exist.");

        var platformModel = _mapper.Map<PlatformModel>(platformEntity);
        await localizer.LocalizePlatformsAsync([platformModel]);

        return platformModel;
    }

    public async Task<List<PlatformModel>> GetAllPlatformsAsync()
    {
        var platformEntities = await _unitOfWork.PlatformRepository.GetAllAsync();

        var platformModels = _mapper.Map<List<PlatformModel>>(platformEntities);
        await localizer.LocalizePlatformsAsync(platformModels);

        return platformModels;
    }

    /// <summary>
    /// The type is saved in the language of the request.
    /// </summary>
    public async Task UpdatePlatformAsync(PlatformModel platformModel)
    {
        var platformEntity = await _unitOfWork.PlatformRepository.GetByIdAsync(platformModel.Id)
            ?? throw new NotFoundException($"Platform with id '{platformModel.Id}' does not exist.");

        if (localizer.IsDefaultLanguage)
        {
            platformEntity.Type = platformModel.Type;
        }
        else
        {
            await localizer.SavePlatformTranslationAsync(platformEntity, platformModel.Type);
        }

        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// The translations of the platform are removed by the database cascade.
    /// </summary>
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

        var gameEntities = await _unitOfWork.ProductPlatformRepository.GetGamesByPlatformIdAsync(platformId);

        var gameModels = _mapper.Map<List<GameModel>>(gameEntities);
        await localizer.LocalizeGamesAsync(gameModels);

        return gameModels;
    }
}
