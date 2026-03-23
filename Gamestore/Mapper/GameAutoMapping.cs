using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.BanModels;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Models.OrderModels;
using BusinessLogicLayer.Models.Payment;
using DataAccessLayer.Entities;
using GameStore.Shared.Enums;
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

namespace Gamestore.WebAPI.Mapper;

public class GameAutoMapping : Profile
{
    public GameAutoMapping()
    {
        CreateMap<GameModel, GameEntity>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                   .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                   .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                   .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                   .ForMember(dest => dest.UnitInStock, opt => opt.MapFrom(src => src.UnitInStock))
                   .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                   .ForMember(dest => dest.PublisherId, opt => opt.MapFrom(src => src.Publisher))
                   .ForMember(dest => dest.GameGenres, opt => opt.MapFrom(src =>
               src.Genres.Select(gid => new GameGenreEntity { GenreId = gid })))
               .ForMember(dest => dest.GamePlatforms, opt => opt.MapFrom(src =>
               src.Platforms.Select(pid => new GamePlatformEntity { PlatformId = pid })))
               .ForMember(dest => dest.Publisher, opt => opt.Ignore());

        CreateMap<GameEntity, GameModel>()
    .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
    src.GameGenres != null ? src.GameGenres.Select(gg => gg.GenreId) : new List<Guid>()))
    .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src =>
    src.GamePlatforms != null ? src.GamePlatforms.Select(gp => gp.PlatformId) : new List<Guid>()))
    .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.PublisherId))
    ;

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

        CreateMap<CreateGenreDTO, GenreModel>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
    .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.Parent)).ReverseMap();

        CreateMap<GenreNameAndParentGenreIdDTO, GenreModel>().ReverseMap();

        CreateMap<GenreModel, GenreEntity>().ReverseMap();

        CreateMap<GenreDTO, GenreModel>().ReverseMap();

        CreateMap<FullGenreDTO, GenreModel>()
     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
     .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Parent))
     .ReverseMap();

        CreateMap<UpdateGenreDTO, GenreModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Genre.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
            .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.Parent)).ReverseMap();

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

        CreateMap<PublisherModel, PublisherEntity>().ReverseMap();

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

        // Visa
        CreateMap<VisaPaymentDTO, VisaPaymentModel>()
            .ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.Holder))
            .ForMember(dest => dest.ExpirationMonth, opt => opt.MapFrom(src => src.MonthExpire))
            .ForMember(dest => dest.ExpirationYear, opt => opt.MapFrom(src => src.YearExpire))
            .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => src.Cvv2))
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

        // OrderEntity->OrderModel
        CreateMap<OrderEntity, OrderModel>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderGames));

        // OrderGameEntity -> OrderGameModel
        CreateMap<OrderGameEntity, OrderGameModel>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game != null ? src.Game.Name : string.Empty));

        CreateMap<OrderModel, OrderEntity>()
            .ForMember(dest => dest.OrderGames, opt => opt.MapFrom(src => src.Items));

        CreateMap<OrderGameModel, OrderGameEntity>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount));

        // OrderModel <-> OrderDTO
        CreateMap<OrderModel, OrderDTO>();
        CreateMap<OrderDTO, OrderModel>();

        CreateMap<OrderGameModel, OrderGameDTO>()
    .ForMember(
        dest => dest.Price,
        opt => opt.MapFrom(src =>
            src.Quantity *
            src.Price *
            (1 - ((src.Discount ?? 0) / 100.0))));

        // Request
        CreateMap<PaymentRequestDTO, PaymentRequestModel>()
     .ForMember(
         dest => dest.Method,
         opt => opt.MapFrom(src => src.Method.ToEnum<PaymentOptions>()));

        // Response
        CreateMap<PaymentResponseModel, PaymentResponseDTO>()
            .ForMember(dest => dest.BankResponseDTO, opt => opt.MapFrom(src => src.BankResponseModel))
            .ForMember(dest => dest.IBoxResponseDTO, opt => opt.MapFrom(src => src.IBoxResponseModel));

        // Nested responses
        CreateMap<BankResponseModel, BankResponseDTO>();

        CreateMap<PaymentMethodResponseDTO, PaymentMethodResponseModel>().ReverseMap();

        // CommentEntity → CommentModel
        // =============================
        CreateMap<CommentEntity, CommentModel>();

        // =============================
        // CommentModel → CommentEntity
        // =============================
        CreateMap<CommentModel, CommentEntity>();

        // =============================
        // AddCommentModel → CommentEntity
        // =============================
        CreateMap<AddCommentModel, CommentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GameId, opt => opt.Ignore())
            .ForMember(dest => dest.Game, opt => opt.Ignore())
             .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentId));

        // DTO → Model
        CreateMap<BanRequestDTO, BanModel>()
    .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration.ToEnum<BanDurationOption>()));

        // =============================
        // BanModel ↔ BanEntity
        // =============================
        CreateMap<BanModel, BanEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.ExpireAt, opt => opt.Ignore());

        CreateMap<BanEntity, BanModel>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.UserName));

        CreateMap<AddCommentRequestDTO, AddCommentModel>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Comment.Name))
    .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Comment.Body))

    .ForMember(dest => dest.Action, opt => opt.MapFrom(src =>
        string.IsNullOrWhiteSpace(src.Action) ? null : src.Action))
    .ForMember(dest => dest.ParentId, opt => opt.MapFrom((src, dest) =>
    {
        return string.IsNullOrWhiteSpace(src.ParentId) ? null : Guid.TryParse(src.ParentId, out var guid) ? guid : (Guid?)null;
    }));

        // FilterDTO -> FilterModel
        // ================================
        CreateMap<GameFilterDTO, GameFilterModel>();

        // ================================
        // ResultModel -> ResultDto
        // ================================
        CreateMap<FilteredGamesResponseModel, FilteredGamesResponseDTO>()
            .ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.Games));
    }
}