using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ICommentSqlRepository : ISqlServerGenericRepository<CommentEntity, Guid>
{
}
