namespace BusinessLogicLayer.Catalog.Interfaces;

/// <summary>
/// Single entry point to the catalogs that merge Game Store and Northwind data.
/// </summary>
public interface IStoreCatalog
{
    IProductCatalog Products { get; }

    ICategoryCatalog Categories { get; }

    ISupplierCatalog Suppliers { get; }
}
