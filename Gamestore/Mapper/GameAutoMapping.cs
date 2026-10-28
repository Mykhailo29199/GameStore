using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Models.OrderModels;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using GameStore.Shared.Enums;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Enums.PaymentEnums;
using GameStore.Shared.Extensions;
using Gamestore.WebAPI.DTOs.CommentDTOs;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Gamestore.WebAPI.DTOs.OrderDTO;
using Gamestore.WebAPI.DTOs.PaymentDTO;
using Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Gamestore.WebAPI.DTOs.PublisherDTO;
using Gamestore.WebAPI.DTOs.Users;
using BanDurationOption = GameStore.Shared.Enums.BanDurationOption;
using BanModel = BusinessLogicLayer.Models.BanModels.BanModel;

namespace Gamestore.WebAPI.Mapper;

public class GameAutoMapping : Profile
{
    public GameAutoMapping()
    {
        // Games
        CreateMap<GameModel, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.UnitsInStock, opt => opt.MapFrom(src => src.UnitInStock))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.SupplierID, opt => opt.MapFrom(src => src.Publisher))
            .ForMember(dest => dest.ProductCategories, opt => opt.MapFrom(src =>
                src.Genres.Select(gid => new ProductCategoryEntity { CategoryId = gid })))
            .ForMember(dest => dest.ProductPlatforms, opt => opt.MapFrom(src =>
                src.Platforms.Select(pid => new ProductPlatformEntity { PlatformId = pid })))
            .ForMember(dest => dest.Supplier, opt => opt.Ignore());

        CreateMap<ProductEntity, GameModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.UnitInStock, opt => opt.MapFrom(src => src.UnitsInStock))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.CategoryId)))
            .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src => src.ProductPlatforms.Select(pp => pp.PlatformId)))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.SupplierID));

        CreateMap<GameWithIdDTO, GameModel>()
            .IncludeBase<GameDTO, GameModel>()
            .ReverseMap();

        CreateMap<UpdateGameDTO, GameModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Game.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Game.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Game.Key))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Game.Description))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Game.Price))
            .ForMember(dest => dest.UnitInStock, opt => opt.MapFrom(src => src.Game.UnitInStock))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Game.Discount))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
            .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src => src.Platforms))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
            .ReverseMap();

        CreateMap<CreateGameDTO, GameModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Game.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Game.Key))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Game.Description))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Game.Price))
            .ForMember(dest => dest.UnitInStock, opt => opt.MapFrom(src => src.Game.UnitInStock))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Game.Discount))
            .ReverseMap();

        CreateMap<GameDTO, GameModel>()
            .ReverseMap();

        // Genres
        CreateMap<CreateGenreDTO, GenreModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
            .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.Parent))
            .ReverseMap();

        CreateMap<GenreNameAndParentGenreIdDTO, GenreModel>().ReverseMap();

        CreateMap<GenreModel, CategoryEntity>()
            .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentGenreId))
            .ForMember(dest => dest.ParentGenre, opt => opt.Ignore());

        CreateMap<CategoryEntity, GenreModel>()
            .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.ParentCategoryId));

        CreateMap<GenreDTO, GenreModel>().ReverseMap();

        CreateMap<FullGenreDTO, GenreModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Parent))
            .ReverseMap();

        CreateMap<UpdateGenreDTO, GenreModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Genre.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
            .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.Parent))
            .ReverseMap();

        // Platforms
        CreateMap<FullPlatformDTO, PlatformModel>().ReverseMap();

        CreateMap<UpdatePlatformDTO, PlatformModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Platform.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Platform.Type))
            .ReverseMap();

        CreateMap<PlatformDTO, PlatformModel>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ReverseMap();

        CreateMap<CreatePlatformDTO, PlatformModel>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Platform.Type))
            .ReverseMap();

        CreateMap<PlatformModel, PlatformEntity>().ReverseMap();

        // Publishers
        CreateMap<PublisherModel, SupplierEntity>().ReverseMap();

        CreateMap<PublisherDTO, PublisherModel>().ReverseMap();

        CreateMap<CreatePublisherDTO, PublisherModel>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Publisher.CompanyName))
            .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.Publisher.HomePage))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Publisher.Description))
            .ReverseMap();

        CreateMap<PublisherWithIdDTO, PublisherModel>().ReverseMap();

        CreateMap<UpdatePublisherDTO, PublisherModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Publisher.Id))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Publisher.CompanyName))
            .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.Publisher.HomePage))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Publisher.Description))
            .ReverseMap();

        // Payments
        CreateMap<VisaPaymentDTO, VisaPaymentModel>()
            .ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.Holder))
            .ForMember(dest => dest.ExpirationMonth, opt => opt.MapFrom(src => src.MonthExpire))
            .ForMember(dest => dest.ExpirationYear, opt => opt.MapFrom(src => src.YearExpire))
            .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => src.Cvv2))
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

        CreateMap<PaymentRequestDTO, PaymentRequestModel>()
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToEnum<PaymentOptions>()));

        CreateMap<PaymentResponseModel, PaymentResponseDTO>()
            .ForMember(dest => dest.BankResponseDTO, opt => opt.MapFrom(src => src.BankResponseModel))
            .ForMember(dest => dest.IBoxResponseDTO, opt => opt.MapFrom(src => src.IBoxResponseModel));

        CreateMap<BankResponseModel, BankResponseDTO>();

        CreateMap<PaymentMethodResponseDTO, PaymentMethodResponseModel>().ReverseMap();

        // Orders
        CreateMap<DataAccessLayer.Entities.NorthwindMDBEntities.OrderMongoDBEntity, OrderModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => MongoIdConverter.ToGuid(s.InternalId)))
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.CustomerID ?? string.Empty))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => OrderStatus.Paid));

        CreateMap<OrderEntity, OrderModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.CustomerId.ToString()))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status));

        CreateMap<OrderModel, OrderDTO>();

        CreateMap<OrderDetailsEntity, OrderGameModel>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount.HasValue ? (int?)Math.Round(src.Discount.Value) : null))
            .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty));

        CreateMap<OrderGameModel, OrderGameDTO>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => OrderTotalCalculator.LineTotal(src.Price, src.Quantity, src.Discount)));

        // Comments
        CreateMap<CommentEntity, CommentModel>();

        CreateMap<CommentModel, CommentEntity>();

        CreateMap<AddCommentModel, CommentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentId));

        CreateMap<AddCommentRequestDTO, AddCommentModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Comment.Name))
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Comment.Body))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Action) ? null : src.Action))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom((src, dest) => ParseOptionalGuid(src.ParentId)));

        // Bans
        CreateMap<BanRequestDTO, BanModel>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration.ToEnum<BanDurationOption>()));

        CreateMap<BanModel, BanEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.ExpireAt, opt => opt.Ignore());

        CreateMap<BanEntity, BanModel>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.UserName));

        // Game filters
        CreateMap<GameFilterDTO, GameFilterModel>()
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => ParsePageSize(src.PageCount)))
            .ForMember(dest => dest.DatePublishing, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.DatePublishing)
                    ? PublishDateOptions.AllTime
                    : src.DatePublishing.ToEnum<PublishDateOptions>()));

        CreateMap<FilteredGamesResponseModel, FilteredGamesResponseDTO>()
            .ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.Games));
    }

    private static Guid? ParseOptionalGuid(string? value)
    {
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private static int ParsePageSize(string? pageCount)
    {
        if (string.IsNullOrWhiteSpace(pageCount))
        {
            return (int)PaginationOptions.Ten;
        }

        return pageCount == "All" ? (int)PaginationOptions.All : int.Parse(pageCount);
    }
}
