using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class PublisherService(
    ISqlServerUnitOfWork unitOfWork,
    IMapper mapper,
    ISupplierCatalog supplierCatalog,
    IProductCatalog productCatalog) : IPublisherService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreatePublisherAsync(PublisherModel publisherModel)
    {
        var exist = await supplierCatalog.GetByCompanyNameAsync(publisherModel.CompanyName);

        if (exist != null)
        {
            throw new ArgumentException($"Publisher with this name '{publisherModel.CompanyName}' already exists");
        }

        var publisherEntity = _mapper.Map<SupplierEntity>(publisherModel);
        await _unitOfWork.SupplierRepository.AddAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<PublisherModel> GetPublisherByCompanyNameAsync(string companyName)
    {
        var publisherEntity = await supplierCatalog.GetByCompanyNameAsync(companyName)
            ?? throw new NotFoundException($"Publisher with this name '{companyName}' does not exist");

        var publisherModel =
      _mapper.Map<PublisherModel>(publisherEntity);

        return publisherModel;
    }

    public async Task<List<PublisherModel>> GetPublishersAsync()
    {
        var publisherEntities = await supplierCatalog.GetAllAsync();

        var publisherModels = _mapper.Map<List<PublisherModel>>(publisherEntities);

        return publisherModels;
    }

    /// <summary>
    /// A Northwind publisher is copied to the Game Store database and the copy is updated; Northwind data stays untouched.
    /// </summary>
    public async Task UpdatePublisherAsync(PublisherModel publisherModel)
    {
        _ = await supplierCatalog.GetByIdAsync(publisherModel.Id)
            ?? throw new NotFoundException($"Publisher with id '{publisherModel.Id}' does not exist.");

        var exist = await supplierCatalog.GetByCompanyNameAsync(publisherModel.CompanyName);

        if (exist != null && exist.Id != publisherModel.Id)
        {
            throw new BadRequestException($"Publisher with company name '{publisherModel.CompanyName}' already exists.");
        }

        var publisherEntity = await supplierCatalog.EnsureInGameStoreAsync(publisherModel.Id);

        _mapper.Map(publisherModel, publisherEntity);

        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Publishers that exist in Northwind are soft-deleted in the Game Store database, so the Northwind original stays hidden.
    /// </summary>
    public async Task DeletePublisherAsync(Guid publisherId)
    {
        var publisher = await supplierCatalog.GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with id '{publisherId}' does not exist.");

        if (!await supplierCatalog.HasNorthwindCounterpartAsync(publisher))
        {
            var publisherEntity = await _unitOfWork.SupplierRepository.GetByIdAsync(publisherId)
                ?? throw new NotFoundException($"Publisher with id '{publisherId}' does not exist.");

            await _unitOfWork.SupplierRepository.DeleteAsync(publisherEntity);
        }
        else
        {
            var gameStorePublisher = await supplierCatalog.EnsureInGameStoreAsync(publisherId);
            gameStorePublisher.IsDeleted = true;
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByPublisherNameAsync(string publisherName)
    {
        var publisher = await supplierCatalog.GetByCompanyNameAsync(publisherName)
            ?? throw new NotFoundException($"Publisher '{publisherName}' not found.");

        return await GetGamesByPublisherAsync(publisher.Id);
    }

    public async Task<List<GameModel>> GetGamesByPublisherIdAsync(Guid publisherId)
    {
        _ = await supplierCatalog.GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with id '{publisherId}' not found.");

        return await GetGamesByPublisherAsync(publisherId);
    }

    private async Task<List<GameModel>> GetGamesByPublisherAsync(Guid publisherId)
    {
        var games = (await productCatalog.GetAllAsync())
            .Where(g => g.SupplierID == publisherId)
            .ToList();

        return _mapper.Map<List<GameModel>>(games);
    }
}
