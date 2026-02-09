using AutoMapper;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Gamestore.WebAPI.DTOs.PublisherDTO;

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

        CreateMap<GameWithIdDto, GameModel>() // +
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

        // CreateMap<CreateGameDTO, GameModel>().ReverseMap();   // +

        CreateMap<CreateGenreDTO, GenreModel>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
    .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.Parent)).ReverseMap();

        CreateMap<GenreNameAndParentGenreIdDto, GenreModel>().ReverseMap(); // +

        CreateMap<GenreModel, GenreEntity>().ReverseMap(); // +

        CreateMap<GenreDTO, GenreModel>().ReverseMap(); // +

        CreateMap<FullGenreDTO, GenreModel>() // +
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

        // --- Мапінг Publisher ---  ++++++++++++++++
        CreateMap<PublisherModel, PublisherEntity>().ReverseMap();

        CreateMap<PublisherDTO, PublisherModel>().ReverseMap();
        CreateMap<CreatePublisherDTO, PublisherModel>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Publisher.CompanyName))
            .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.Publisher.HomePage))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Publisher.Description))
            .ReverseMap();

        // Мапінг для PublisherWithIdDTO (вкладений PublisherDTO -> плаский PublisherModel)
        CreateMap<PublisherWithIdDTO, PublisherModel>().ReverseMap();
        CreateMap<UpdatePublisherDTO, PublisherModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Publisher.Id))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Publisher.CompanyName))
            .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.Publisher.HomePage))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Publisher.Description))
            .ReverseMap();
    }
}
