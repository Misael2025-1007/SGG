using Microsoft.EntityFrameworkCore;
using SGG.Domain.Base;
using SGG.Domain.Repository;
using SGG.Persistence.Context;
using System.Linq.Expressions;

namespace SGG.Persistence.Base
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly SGGContext _context;
        private DbSet<TEntity> Entity { get; set; }

        protected BaseRepository(SGGContext context)
        {
            _context = context;
            Entity = _context.Set<TEntity>();
        }

        public virtual async Task<OperationResult> SaveEntityAsync(TEntity entity)
        {
            OperationResult result = new OperationResult();
            try
            {
                Entity.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                result.Success = false;
                result.Message = "Ocurrió un error guardando los datos.";
            }
            return result;
        }

        public virtual async Task<OperationResult> UpdateEntityAsync(TEntity entity)
        {
            OperationResult result = new OperationResult();
            try
            {
                Entity.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                result.Success = false;
                result.Message = "Ocurrió un error actualizando los datos.";
            }
            return result;
        }

        public virtual async Task<OperationResult> RemoveEntityAsync(TEntity entity)
        {
            OperationResult result = new OperationResult();
            try
            {
                Entity.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                result.Success = false;
                result.Message = "Ocurrió un error eliminando los datos.";
            }
            return result;
        }

        public virtual async Task<OperationResult> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            OperationResult result = new OperationResult();
            try
            {
                result.Data = await Entity.Where(filter).ToListAsync();
            }
            catch (Exception)
            {
                result.Success = false;
                result.Message = "Ocurrió un error obteniendo los datos.";
            }
            return result;
        }

        public virtual async Task<TEntity?> GetEntityByIdAsync(int id)
        {
            return await Entity.FindAsync(id);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await Entity.AnyAsync(filter);
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await Entity.ToListAsync();
        }
    }
}
