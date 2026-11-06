namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

/// <summary>
/// A row of the SQL function <c>dbo.GetLocalizedGames</c>: game information in the requested language,
/// or in English when the game has no translation.
/// </summary>
public class LocalizedGameEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public double Discount { get; set; }

    public int UnitsInStock { get; set; }

    public string LanguageCode { get; set; }
}
