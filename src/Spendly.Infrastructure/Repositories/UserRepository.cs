using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SpendlyDbContext _context;

        public UserRepository(SpendlyDbContext context)
        {
            _context = context;
        }

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}
