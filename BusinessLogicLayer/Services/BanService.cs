using AutoMapper;
using BusinessLogicLayer.Models.BanModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.Extensions;

namespace BusinessLogicLayer.Services;

public class BanService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IBanService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public List<string> GetBanDurations()
    {
        return Enum.GetValues(typeof(BanDurationOption))
         .Cast<BanDurationOption>()
         .Select(d => d.GetDescription())
         .ToList();
    }

    public async Task BanUserAsync(BanModel model)
    {
        var comment = await _unitOfWork.CommentRepository
            .FirstOrDefaultAsync(c => c.Name == model.User) ?? throw new NotFoundException("User with this name has no comments");

        var activeBan = await _unitOfWork.BanRepository
            .FirstOrDefaultAsync(b =>
                b.UserName == model.User &&
                (b.ExpireAt == null || b.ExpireAt > DateTime.UtcNow));

        if (activeBan != null)
        {
            throw new BadRequestException("User already banned");
        }

        DateTime? expireAt = model.Duration switch
        {
            BanDurationOption.OneHour => DateTime.UtcNow.AddHours(1),
            BanDurationOption.OneDay => DateTime.UtcNow.AddDays(1),
            BanDurationOption.OneWeek => DateTime.UtcNow.AddDays(7),
            BanDurationOption.OneMonth => DateTime.UtcNow.AddMonths(1),
            BanDurationOption.Permanent => DateTime.MaxValue,
            _ => throw new BadRequestException("Invalid duration"),
        };

        var banEntity = _mapper.Map<BanEntity>(model);
        banEntity.ExpireAt = expireAt;

        await _unitOfWork.BanRepository.AddAsync(banEntity);
        await _unitOfWork.SaveAsync();
    }
}
