using TruevoExchangeRateAPI.Data.Models;

namespace TruevoExchangeRateAPI.Data.Repository
{
    public interface IRepository<TEntity, in TKey> where TEntity : BaseEntity<TKey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        IQueryable<TEntity> GetAllQuerable();
        Task<TEntity> GetAsync(TKey id);
        IQueryable<TEntity> GetQueryable();
        Task<TEntity> InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
        Task DeleteAllAsync();
    }
}
