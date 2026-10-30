namespace DataAccessLayer.Entities.CommonEntities;

public abstract class TerritoryBaseEntity
{
    public string? TerritoryID { get; set; }

    public string? TerritoryDescription { get; set; }

    public int? RegionID { get; set; }
}