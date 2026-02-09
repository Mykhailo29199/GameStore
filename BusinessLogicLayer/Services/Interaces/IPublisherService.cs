using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPublisherService
{
    Task CreatePublisherAsync(PublisherModel publisherModel);

    Task<PublisherModel> GetPublisherByCompanyNameAsync(string companyName);

    Task<List<PublisherModel>> GetPublishersAsync();

    Task UpdatePublisherAsync(PublisherModel publisherModel);

    Task DeletePublisherAsync(Guid publisherId);

    Task<List<GameModel>> GetGamesByPublisherNameAsync(string publisherName);
}
