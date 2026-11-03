using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TestKaspersky.Models;

namespace TestKaspersky.Data
{
    public interface IRepository<TEntity, in TKey> where TEntity : class where TKey : notnull
    {

        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        public Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default);

        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct);
    }

    public class InMemoryRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TEntity> _store = new();

        public Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(_store.Values.ToList());

        public ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
            => new(_store.GetValueOrDefault(id));

        public Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        {
            var key = GetKey(entity);
            _store[key] = entity;
            return Task.FromResult(entity);
        }

        public Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            _store[GetKey(entity)] = entity;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
            => Task.FromResult(_store.TryRemove(id, out _));

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                foreach (var entity in entities)
                {
                    _store[GetKey(entity)] = entity;
                }
            }, ct);
        }

        // остальные методы (GetPaged, Exists, Count, FirstOrDefault) — по аналогии
        private static TKey GetKey(TEntity entity)
            => (TKey)(typeof(TEntity).GetProperty("Id")!.GetValue(entity)!);
    }

}
