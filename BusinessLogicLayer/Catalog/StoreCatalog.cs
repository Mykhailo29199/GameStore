using BusinessLogicLayer.Catalog.Interfaces;

namespace BusinessLogicLayer.Catalog;

public class StoreCatalog(
    IProductCatalog products,
    ICategoryCatalog categories,
    ISupplierCatalog suppliers) : IStoreCatalog
{
    public IProductCatalog Products { get; } = products;

    public ICategoryCatalog Categories { get; } = categories;

    public ISupplierCatalog Suppliers { get; } = suppliers;
}
