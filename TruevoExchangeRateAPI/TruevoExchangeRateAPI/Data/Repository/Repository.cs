using Microsoft.EntityFrameworkCore;
using TruevoExchangeRateAPI.Data.Models;

namespace TruevoExchangeRateAPI.Data.Repository
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        private readonly TruevoExchangeRateDbContext _context;
        private DbSet<TEntity> _entities;

        public DbSet<TEntity> GetDbSet()
        {
            return _entities;
        }

        public Repository(TruevoExchangeRateDbContext context)
        {
            this._context = context;
            _entities = context.Set<TEntity>();
        }

        public Task<TEntity> GetAsync(TKey id)
        {
            return _entities.SingleOrDefaultAsync(s => s.Id.Equals(id));
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public IQueryable<TEntity> GetAllQuerable()
        {
            return _entities.AsQueryable();
        }

        public virtual IQueryable<TEntity> GetQueryable()
        {
            return _entities;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            _context.Entry(entity).State = EntityState.Modified;
            return _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TKey id)
        {
            var entity = await GetAsync(id);
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            _entities.Remove(entity);
            _ = await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            if (_entities.Any())
            {
                _entities.RemoveRange(_entities.ToList());
                _ = await _context.SaveChangesAsync();
            }
        }
    }
}
