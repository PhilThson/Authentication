using Authentication.Domain.Interfaces.Repositories;
using Authentication.Domain.Models;
using Authentication.Infrastructure.DataAccess;

namespace Authentication.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
	{
        private readonly AuthDbContext _dbContext;
        private ICommonRepository<User> user;

        public UnitOfWork(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ICommonRepository<User> User =>
            user ??= new CommonRepository<User>(_dbContext);

        public void Save() =>
            _dbContext.SaveChanges();

        public async Task SaveAsync() =>
            await _dbContext.SaveChangesAsync();
    }
}

