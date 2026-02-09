using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class PublisherService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IPublisherService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreatePublisherAsync(PublisherModel publisherModel)
    {
        var exist = await _unitOfWork.PublisherRepository
            .FirstOrDefaultAsync(p => p.CompanyName == publisherModel.CompanyName);

        if (exist != null)
        {
            throw new ArgumentException($"Publisher with this name '{publisherModel.CompanyName}' already exists");
        }

        var publisherEntity = _mapper.Map<PublisherEntity>(publisherModel);
        await _unitOfWork.PublisherRepository.AddAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<PublisherModel> GetPublisherByCompanyNameAsync(string companyName)
    {
        var publisherEntity = await _unitOfWork.PublisherRepository
            .FirstOrDefaultAsync(p => p.CompanyName == companyName)
            ?? throw new NotFoundException($"Publisher with this name '{companyName}' does not exist");

        var publisherModel =
      _mapper.Map<PublisherModel>(publisherEntity);

        return publisherModel;
    }

    public async Task<List<PublisherModel>> GetPublishersAsync()
    {
        var publisherEntities = await _unitOfWork.PublisherRepository.GetAllAsync();

        var publisherModels = _mapper.Map<List<PublisherModel>>(publisherEntities);

        return publisherModels;
    }

    public async Task UpdatePublisherAsync(PublisherModel publisherModel)
    {
        var publisherEntity = await _unitOfWork.PublisherRepository
            .GetByIdAsync(publisherModel.Id)
            ?? throw new NotFoundException($"Publisher with id '{publisherModel.Id}' does not exist.");

        var exist = await _unitOfWork.PublisherRepository.FirstOrDefaultAsync(p =>
            p.CompanyName == publisherModel.CompanyName &&
            p.Id != publisherModel.Id);

        _mapper.Map(publisherModel, publisherEntity);

        await _unitOfWork.PublisherRepository.UpdateAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeletePublisherAsync(Guid publisherId)
    {
        var publisherEntity = await _unitOfWork.PublisherRepository
            .GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with id '{publisherId}' does not exist.");

        await _unitOfWork.PublisherRepository.DeleteAsync(publisherEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<GameModel>> GetGamesByPublisherNameAsync(string publisherName)
    {
        var publisher = await _unitOfWork.PublisherRepository
        .FirstOrDefaultAsync(p => p.CompanyName == publisherName)
        ?? throw new NotFoundException($"Publisher '{publisherName}' not found.");

        var games = await _unitOfWork.PublisherRepository
            .GetGamesByPublisherNameAsync(publisherName);

        return _mapper.Map<List<GameModel>>(games);
    }
}
