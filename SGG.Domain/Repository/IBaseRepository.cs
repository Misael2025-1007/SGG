using SGG.Domain.Base;
using System.Linq.Expressions;

namespace SGG.Domain.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetEntityByIdAsync(int id);
        Task<OperationResult> SaveEntityAsync(TEntity entity);
        Task<OperationResult> UpdateEntityAsync(TEntity entity);
        Task<OperationResult> RemoveEntityAsync(TEntity entity);
        Task<List<TEntity>> GetAllAsync();
        Task<OperationResult> GetAllAsync(Expression<Func<TEntity, bool>> filter);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter);
    }
}
