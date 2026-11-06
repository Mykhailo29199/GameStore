using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class CommentService(
    ISqlServerUnitOfWork unitOfWork,
    IMapper mapper,
    IProductCatalog productCatalog,
    ICurrentUser currentUser) : ICommentService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    private readonly string _deletedMessage = "A comment/quote was deleted";

    // Comments of a deleted game can be managed only by an administrator.
    private bool CanManageDeletedGameComments => currentUser.HasPermission(Permissions.ManageDeletedGameComments);

    public async Task AddCommentAsync(string gameKey, AddCommentModel model)
    {
        _ = await productCatalog.GetByKeyAsync(gameKey, CanManageDeletedGameComments)
            ?? throw new NotFoundException("Game not found");

        // The user name is used as the commenter name.
        var userName = currentUser.Name
            ?? throw new UnauthorizedException("Only authorized users can comment games.");

        var ban = await _unitOfWork.BanRepository.FirstOrDefaultAsync(b =>
            b.UserName == userName &&
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

        // Commenting a Northwind game copies it to the Game Store database, Northwind stays untouched.
        var game = await productCatalog.EnsureInGameStoreAsync(gameKey, CanManageDeletedGameComments);

        var entity = _mapper.Map<CommentEntity>(model);
        entity.Id = Guid.NewGuid();
        entity.ProductId = game.Id;
        entity.Name = userName;
        entity.Body = body;

        await _unitOfWork.CommentRepository.AddAsync(entity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<CommentModel>> GetGameCommentsAsync(string gameKey)
    {
        var game = await productCatalog.GetByKeyAsync(gameKey, currentUser.HasPermission(Permissions.ViewDeletedGames))
            ?? throw new NotFoundException("Game not found");

        var entities = await _unitOfWork.CommentRepository
            .FindAsync(c => c.ProductId == game.Id);

        var models = _mapper.Map<List<CommentModel>>(entities);

        return BuildTree(models);
    }

    public async Task DeleteCommentAsync(string gameKey, Guid id)
    {
        var game = await productCatalog.GetByKeyAsync(gameKey, CanManageDeletedGameComments)
            ?? throw new NotFoundException("Game not found");

        var comment = await _unitOfWork.CommentRepository
            .GetByIdAsync(id)
            ?? throw new NotFoundException("Comment not found");

        if (comment.ProductId != game.Id)
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