using AutoMapper;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class CommentService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : ICommentService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    private readonly string _deletedMessage = "A comment/quote was deleted";

    public async Task AddCommentAsync(string gameKey, AddCommentModel model)
    {
        var game = await _unitOfWork.GameRepository
            .FirstOrDefaultAsync(g => g.Key == gameKey)
            ?? throw new NotFoundException("Game not found");

        var ban = await _unitOfWork.BanRepository.FirstOrDefaultAsync(b =>
            b.UserName == model.Name &&
            (b.ExpireAt > DateTime.UtcNow));

        if (ban != null)
        {
            throw new BadRequestException("User is banned");
        }

        string body = model.Body;

        if (model.ParentId.HasValue)
        {
            var parent = await _unitOfWork.CommentRepository
                .GetByIdAsync(model.ParentId.Value)
                ?? throw new NotFoundException("Parent comment not found");

            if (model.Action == CommentActions.Reply)
            {
                body = $"[{parent.Name}], {body}";
            }
            else if (model.Action == CommentActions.Quote)
            {
                var quotedText = parent.Body;
                if (string.IsNullOrWhiteSpace(quotedText))
                {
                    quotedText = _deletedMessage;
                }

                body = $"> {quotedText}\n\n{body}";
            }
        }

        var entity = _mapper.Map<CommentEntity>(model);
        entity.Id = Guid.NewGuid();
        entity.GameId = game.Id;
        entity.Body = body;

        await _unitOfWork.CommentRepository.AddAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<CommentModel>> GetGameCommentsAsync(string gameKey)
    {
        var game = await _unitOfWork.GameRepository
            .FirstOrDefaultAsync(g => g.Key == gameKey)
            ?? throw new NotFoundException("Game not found");

        var entities = await _unitOfWork.CommentRepository
            .FindAsync(c => c.GameId == game.Id);

        var models = _mapper.Map<List<CommentModel>>(entities);

        return BuildTree(models);
    }

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
            throw new BadRequestException("Comment does not belong to this game");
        }

        comment.Body = _deletedMessage;
        comment.IsDeleted = true;

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