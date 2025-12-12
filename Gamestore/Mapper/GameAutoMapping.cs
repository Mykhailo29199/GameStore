using AutoMapper;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;

namespace Gamestore.WebAPI.Mapper;

public class GameAutoMapping : Profile
{
    public GameAutoMapping()
    {
        CreateMap<GameModel, GameEntity>()
            .ForMember(dest => dest.GameKey, opt => opt.MapFrom(src => src.Key));

        CreateMap<FullGameModel, GameEntity>()
               .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name))
               .ForMember(dest => dest.GameKey, opt => opt.MapFrom(src => src.Game.Key))
               .ForMember(dest => dest.GameDescription, opt => opt.MapFrom(src => src.Game.Description))
               .ForMember(dest => dest.GameGenres, opt => opt.MapFrom(src =>
               src.Genres.Select(gid => new GameGenreEntity { GenreId = gid })))
               .ForMember(dest => dest.GamePlatforms, opt => opt.MapFrom(src =>
               src.Platforms.Select(pid => new GamePlatformEntity { PlatformId = pid })));

        CreateMap<GameEntity, GameModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GameId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GameName))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GameKey))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.GameDescription));

        CreateMap<GameModel, GameWithIdDTO>()
                .ReverseMap();

        CreateMap<GameEntity, FullGameModel>()
            .ForMember(dest => dest.Game, opt => opt.MapFrom(src => new GameModel
            {
                Id = src.GameId,
                Name = src.GameName,
                Key = src.GameKey,
                Description = src.GameDescription,
            }))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
            src.GameGenres != null ? src.GameGenres.Select(gg => gg.GenreId) : new List<Guid>()))
            .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src =>
            src.GamePlatforms != null ? src.GamePlatforms.Select(gp => gp.PlatformId) : new List<Guid>()));

        CreateMap<GameWithIdDTO, GameModel>();

        CreateMap<FullGameWithIdDTO, FullGameModel>()
            .ForMember(dest => dest.Game, opt => opt.MapFrom(src => src.GameWithIdDTO))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
            .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src => src.Platforms));

        CreateMap<GameDTO, GameModel>()
                .ReverseMap();

        CreateMap<FullGameDTO, FullGameModel>()
                .ReverseMap();

        CreateMap<GenreNameAndParentGenreIdDTO, GenreModel>().ReverseMap();

        CreateMap<GenreModel, GenreEntity>().ReverseMap();

        CreateMap<GenreDTO, GenreModel>().ReverseMap();

        CreateMap<FullGenreDTO, GenreModel>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name))
    .ForMember(dest => dest.ParentGenreId, opt => opt.MapFrom(src => src.Genre.ParentGenreId));

        CreateMap<GenreModel, FullGenreDTO>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => new GenreNameAndParentGenreIdDTO
    {
        Name = src.Name,
        ParentGenreId = src.ParentGenreId,
    }));

        CreateMap<PlatformDTO, PlatformModel>().ReverseMap();
        CreateMap<PlatformModel, PlatformEntity>().ReverseMap();
    }
}
