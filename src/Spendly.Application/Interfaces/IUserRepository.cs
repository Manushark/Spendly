using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);

        /// <summary>Returns all users (used by background services like weekly digest).</summary>
        Task<IEnumerable<User>> GetAllAsync();
    }
}

