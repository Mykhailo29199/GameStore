using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using Northwind = DataAccessLayer.Entities.NorthwindMDBEntities;

namespace BusinessLogicLayer.Catalog;

/// <summary>
/// Converts Northwind documents into Game Store entities, so the rest of the application works with a single model.
/// The Guid of a converted entity is derived from its ObjectId, so a copy saved to SQL keeps the same identity.
/// </summary>
public static class NorthwindEntityConverter
{
    public static CategoryEntity ToCategory(Northwind.CategoryEntity category)
    {
        return new CategoryEntity
        {
            Id = MongoIdConverter.ToGuid(category.InternalId),
            CategoryID = category.CategoryID,
            Name = category.Name?.Trim() ?? string.Empty,
            Description = category.Description,
            Picture = category.Picture,
        };
    }

    public static SupplierEntity ToSupplier(Northwind.SupplierEntity supplier)
    {
        return new SupplierEntity
        {
            Id = MongoIdConverter.ToGuid(supplier.InternalId),
            SupplierID = supplier.SupplierID,
            CompanyName = supplier.CompanyName?.Trim() ?? string.Empty,
            ContactName = supplier.ContactName,
            ContactTitle = supplier.ContactTitle,
            Address = supplier.Address,
            City = supplier.City,
            Region = supplier.Region,
            PostalCode = supplier.PostalCode,
            Country = supplier.Country,
            Phone = supplier.Phone,
            Fax = supplier.Fax,
            HomePage = supplier.HomePage,
        };
    }

    public static ProductEntity ToProduct(
        Northwind.ProductEntity product,
        SupplierEntity supplier,
        CategoryEntity? category)
    {
        var id = MongoIdConverter.ToGuid(product.InternalId);

        var entity = new ProductEntity
        {
            Id = id,
            ProductID = product.ProductID,
            ProductName = product.ProductName ?? string.Empty,
            Key = product.Key,
            UnitPrice = product.UnitPrice,
            UnitsInStock = Math.Max(product.UnitsInStock, 0),
            UnitsOnOrder = product.UnitsOnOrder,
            ReorderLevel = product.ReorderLevel,
            QuantityPerUnit = product.QuantityPerUnit,
            Discontinued = product.Discontinued,
            Views = product.Views ?? 0,
            CreatedAt = product.InternalId.CreationTime,
            SupplierID = supplier.Id,
            Supplier = supplier,
        };

        if (category is not null)
        {
            entity.ProductCategories.Add(new ProductCategoryEntity
            {
                ProductId = id,
                CategoryId = category.Id,
                Category = category,
            });
        }

        return entity;
    }
}
