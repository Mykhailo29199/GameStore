using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class PublisherService(ISqlServerUnitOfWork unitOfWork, IMapper mapper) : IPublisherService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreatePublisherAsync(PublisherModel publisherModel)
    {
        var exist = await _unitOfWork.SupplierRepository
            .FirstOrDefaultAsync(p => p.CompanyName == publisherModel.CompanyName);

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
        var publisherEntity = await _unitOfWork.SupplierRepository
            .FirstOrDefaultAsync(p => p.CompanyName == companyName)
            ?? throw new NotFoundException($"Publisher with this name '{companyName}' does not exist");

        var publisherModel =
      _mapper.Map<PublisherModel>(publisherEntity);

        return publisherModel;
    }

    public async Task<List<PublisherModel>> GetPublishersAsync()
    {
        var publisherEntities = await _unitOfWork.SupplierRepository.GetAllAsync();

        var publisherModels = _mapper.Map<List<PublisherModel>>(publisherEntities);

        return publisherModels;
    }

    public async Task UpdatePublisherAsync(PublisherModel publisherModel)
    {
        var publisherEntity = await _unitOfWork.SupplierRepository
            .GetByIdAsync(publisherModel.Id)
            ?? throw new NotFoundException($"Publisher with id '{publisherModel.Id}' does not exist.");

        var exist = await _unitOfWork.SupplierRepository.FirstOrDefaultAsync(p =>
            p.CompanyName == publisherModel.CompanyName &&
            p.Id != publisherModel.Id);

        if (exist != null)
        {
            throw new ValidationException($"Publisher with company name '{publisherModel.CompanyName}' already exists.");
        }

        _mapper.Map(publisherModel, publisherEntity);

        await _unitOfWork.SupplierRepository.UpdateAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeletePublisherAsync(Guid publisherId)
    {
        var publisherEntity = await _unitOfWork.SupplierRepository
            .GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with id '{publisherId}' does not exist.");

        await _unitOfWork.SupplierRepository.DeleteAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByPublisherNameAsync(string publisherName)
    {
        _ = await _unitOfWork.SupplierRepository
        .FirstOrDefaultAsync(p => p.CompanyName == publisherName)
        ?? throw new NotFoundException($"Publisher '{publisherName}' not found.");

        var games = await _unitOfWork.SupplierRepository
            .GetProductsByPublisherNameAsync(publisherName);

        return _mapper.Map<List<GameModel>>(games);
    }

    public async Task<List<GameModel>> GetGamesByPublisherIdAsync(Guid publisherId)
    {
        _ = await _unitOfWork.SupplierRepository
        .GetByIdAsync(publisherId)
        ?? throw new NotFoundException($"Publisher with id '{publisherId}' not found.");

        var games = await _unitOfWork.SupplierRepository
            .GetProductsByPublisherIdAsync(publisherId);

        return _mapper.Map<List<GameModel>>(games);
    }
}
