using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TestKaspersky.Data
{
    public interface IRepository<TEntity, in TKey> where TEntity : class where TKey : notnull
    {

        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    }

    public class Repository<TEntity, TKey>(AppDbContext dbContext) : IRepository<TEntity, TKey> where TEntity : class where TKey : notnull
    {
        private AppDbContext DbContext { get; } = dbContext;

        protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

        public virtual Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => Set.AsNoTracking().ToListAsync(cancellationToken);

        public virtual ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
            => Set.FindAsync([id], cancellationToken);

        public virtual Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => Set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

        public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
            => await Set.FindAsync([id], cancellationToken) is not null;

        public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Set.CountAsync(cancellationToken);

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Set.Add(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            Set.AddRange(entities);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Set.Update(entity);
            return await DbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await Set.FindAsync([id], cancellationToken);
            if (entity is null)
            {
                return false;
            }

            Set.Remove(entity);
            return await DbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Set.Remove(entity);
            return await DbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
