using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Authentication.Infrastructure.DataAccess;
using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Infrastructure.Repositories
{
    public class CommonRepository<T> : ICommonRepository<T> where T : class
    {
        private readonly AuthDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public CommonRepository(AuthDbContext dbContext)
        {
            _context = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public virtual Task<List<T>> GetAllAsync() =>
            _dbSet.AsNoTracking().ToListAsync();

        public virtual Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.AsNoTracking().Where(predicate).ToListAsync();

        public virtual Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.FirstOrDefaultAsync(predicate);

        public virtual ValueTask<T?> GetByIdAsync(int id) =>
            _dbSet.FindAsync(id);

        public virtual bool Exists(Expression<Func<T, bool>> predicate) =>
            _dbSet.Any(predicate);

        public virtual void Add(T entity) =>
            _dbSet.Add(entity);

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(T entity) =>
            _dbSet.Remove(entity);
    }
}

