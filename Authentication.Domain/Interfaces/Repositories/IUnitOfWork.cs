using Authentication.Domain.Models;

namespace Authentication.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork
	{
		ICommonRepository<User> User { get; }

		Task SaveAsync();
		void Save();
	}
}

