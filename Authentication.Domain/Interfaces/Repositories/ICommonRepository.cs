using System.Linq.Expressions;

namespace Authentication.Domain.Interfaces.Repositories
{
    public interface ICommonRepository<T> where T : class
    {
        void Add(T entity);
        void Delete(T entity);
        bool Exists(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
        ValueTask<T?> GetByIdAsync(int id);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);
        void Update(T entity);
    }
}