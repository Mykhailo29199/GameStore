using AutoMapper;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class CommentService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : ICommentService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    private readonly string _deletedMessage = "A comment/quote was deleted";

    // ============================
    // US1 – Add Comment
    // ============================
    public async Task AddCommentAsync(string gameKey, AddCommentModel model)
    {
        var game = await _unitOfWork.GameRepository
            .FirstOrDefaultAsync(g => g.Key == gameKey)
            ?? throw new NotFoundException("Game not found");

        // BAN CHECK
        var ban = await _unitOfWork.BanRepository.FirstOrDefaultAsync(b =>
            b.UserName == model.Name &&
            (b.ExpireAt == null || b.ExpireAt > DateTime.UtcNow));

        if (ban != null)
        {
            throw new ArgumentException("User is banned");
        }

        string body = model.Body;

        if (model.ParentId.HasValue)
        {
            var parent = await _unitOfWork.CommentRepository
                .GetByIdAsync(model.ParentId.Value)
                ?? throw new NotFoundException("Parent comment not found");

            if (model.Action == "Reply")
            {
                body = $"[{parent.Name}], {body}";
            }
            else if (model.Action == "Quote")
            {
                // Додаємо цитату у markdown-стилі
                var quotedText = parent.Body;
                if (string.IsNullOrWhiteSpace(quotedText))
                {
                    quotedText = "[Comment deleted]";
                }

                body = $"> {quotedText}\n\n{body}";
            }
        }

        var entity = new CommentEntity
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Body = body,
            GameId = game.Id,
            ParentCommentId = model.ParentId,
        };

        await _unitOfWork.CommentRepository.AddAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    // ============================
    // US2 – Get Comments
    // ============================
    public async Task<List<CommentModel>> GetGameCommentsAsync(string gameKey)
    {
        var game = await _unitOfWork.GameRepository
            .FirstOrDefaultAsync(g => g.Key == gameKey)
            ?? throw new Exception("Game not found");

        var entities = await _unitOfWork.CommentRepository
            .FindAsync(c => c.GameId == game.Id);

        var models = _mapper.Map<List<CommentModel>>(entities);

        return BuildTree(models);
    }

    // ============================
    // US3 – Delete (Soft)
    // ============================
    public async Task DeleteCommentAsync(string gameKey, Guid id)
    {
        var game = await _unitOfWork.GameRepository
            .GetGameByKeyAsync(gameKey)
            ?? throw new NotFoundException("Game not found");

        var comment = await _unitOfWork.CommentRepository
            .GetByIdAsync(id)
            ?? throw new NotFoundException("Comment not found");

        if (comment.GameId != game.Id)
        {
            throw new ArgumentException("Comment does not belong to this game");
        }

        comment.Body = _deletedMessage;

        await _unitOfWork.SaveAsync();
    }

    // ============================
    // US4 – Durations
    // ============================
    public List<BanDurationOption> GetBanDurations()
    {
        return new List<BanDurationOption>
        {
        new() { Duration = "1 hour", Value = "1h" },
        new() { Duration = "1 day", Value = "1d" },
        new() { Duration = "1 week", Value = "1w" },
        new() { Duration = "1 month", Value = "1m" },
        };
    }

    // ============================
    // US5 – Ban user
    // ============================
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
            throw new Exception("User already banned");
        }

        DateTime? expireAt = model.Duration switch
        {
            "1 hour" => DateTime.UtcNow.AddHours(1),
            "1 day" => DateTime.UtcNow.AddDays(1),
            "1 week" => DateTime.UtcNow.AddDays(7),
            "1 month" => DateTime.UtcNow.AddMonths(1),
            "permanent" => null,
            _ => throw new Exception("Invalid duration"),
        };

        var banEntity = new BanEntity
        {
            Id = Guid.NewGuid(),
            UserName = model.User,
            ExpireAt = expireAt,
        };

        await _unitOfWork.BanRepository.AddAsync(banEntity);
        await _unitOfWork.SaveAsync();
    }

    private static List<CommentModel> BuildTree(List<CommentModel> comments)
    {
        var lookup = comments.ToLookup(c => c.ParentCommentId);

        List<CommentModel> Build(Guid? parentId)
        {
            return lookup[parentId]
                .Select(c =>
                {
                    c.ChildComments = Build(c.Id);
                    return c;
                })
                .ToList();
        }

        return Build(null);
    }
}