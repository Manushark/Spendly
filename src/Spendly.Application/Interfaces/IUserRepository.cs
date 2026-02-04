using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        void Add(User user);
    }
}
