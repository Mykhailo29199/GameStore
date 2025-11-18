using AutoMapper;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using Gamestore.WebAPI.DTOs;

namespace Gamestore.WebAPI.Mapper;

public class GameAutoMapping : Profile
{
    public GameAutoMapping()
    {
        CreateMap<BaseGameModel, GameEntity>()
            .ForMember(dest => dest.GameKey, opt => opt.MapFrom(src => src.Key));

        CreateMap<GameModel, GameEntity>()
               .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name))
               .ForMember(dest => dest.GameKey, opt => opt.MapFrom(src => src.Game.Key))
               .ForMember(dest => dest.GameDescription, opt => opt.MapFrom(src => src.Game.Description))
               .ForMember(dest => dest.GameGenres, opt => opt.MapFrom(src =>
               src.Genres.Select(gid => new GameGenreEntity { GenreId = gid })))
               .ForMember(dest => dest.GamePlatforms, opt => opt.MapFrom(src =>
               src.Platforms.Select(pid => new GamePlatformEntity { PlatformId = pid })));

        CreateMap<GameEntity, BaseGameModel>()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GameKey));

        CreateMap<GameEntity, GameModel>()
            .ForMember(dest => dest.Game, opt => opt.MapFrom(src => new BaseGameModel
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

        CreateMap<BaseGameDTO, BaseGameModel>()
                .ReverseMap();

        CreateMap<GameCreateRequestDTO, GameModel>()
                .ReverseMap();
    }
}
