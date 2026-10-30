using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class CommentSqlRepository(GameStoreContext context) : GenericSqlRepository<CommentEntity, Guid>(context), ICommentSqlRepository
{
}
