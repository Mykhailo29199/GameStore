namespace BusinessLogicLayer.Services.Interaces;

public interface IShipperService
{
    Task<IEnumerable<IDictionary<string, object?>>> GetShippersAsync();
}
