using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories;

public class CommentRepository(GameStoreContext context) : GenericRepository<CommentEntity>(context), ICommentRepository
{
}
