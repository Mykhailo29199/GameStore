using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPublisherService
{
    Task CreatePublisherAsync(PublisherModel publisherModel);

    Task<PublisherModel> GetPublisherByCompanyNameAsync(string companyName);

    Task<List<PublisherModel>> GetPublishersAsync();

    Task UpdatePublisherAsync(PublisherModel publisherModel);

    Task DeletePublisherAsync(Guid publisherId);

    Task<List<GameModel>> GetGamesByPublisherNameAsync(string publisherName);

    Task<List<GameModel>> GetGamesByPublisherIdAsync(Guid publisherId);
}
