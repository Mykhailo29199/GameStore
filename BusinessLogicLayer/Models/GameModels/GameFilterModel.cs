using GameStore.Shared.Enums.FilterOptionsEnums;

namespace BusinessLogicLayer.Models.GameModels;

public class GameFilterModel
{
    public List<Guid>? Genres { get; set; }

    public List<Guid>? Platforms { get; set; }

    public List<Guid>? Publishers { get; set; }

    public double? MinPrice { get; set; }

    public double? MaxPrice { get; set; }

    public PublishDateOptions? DatePublishing { get; set; }

    public string? Name { get; set; }

    public SortingOptions? Sort { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
