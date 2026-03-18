using AutoMapper;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class BanService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IBanService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public List<BanDurationOption> GetBanDurations()
    {
        return new List<BanDurationOption>
    {
        new() { Duration = "1 hour" },
        new() { Duration = "1 day" },
        new() { Duration = "1 week" },
        new() { Duration = "1 month" },
    };
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
            throw new AlreadyExistsException("User already banned");
        }

        DateTime? expireAt = model.Duration switch
        {
            "1 hour" => DateTime.UtcNow.AddHours(1),
            "1 day" => DateTime.UtcNow.AddDays(1),
            "1 week" => DateTime.UtcNow.AddDays(7),
            "1 month" => DateTime.UtcNow.AddMonths(1),
            "permanent" => null,
            _ => throw new BadRequestException("Invalid duration"),
        };

        var banEntity = _mapper.Map<BanEntity>(model);
        banEntity.ExpireAt = expireAt;

        await _unitOfWork.BanRepository.AddAsync(banEntity);
        await _unitOfWork.SaveAsync();
    }
}
