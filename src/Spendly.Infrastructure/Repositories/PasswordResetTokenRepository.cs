using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly SpendlyDbContext _ctx;

        public PasswordResetTokenRepository(SpendlyDbContext ctx) => _ctx = ctx;

        public async Task AddAsync(PasswordResetToken token)
        {
            await _ctx.PasswordResetTokens.AddAsync(token);
            await _ctx.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
            => await _ctx.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

        public async Task UpdateAsync(PasswordResetToken token)
        {
            _ctx.PasswordResetTokens.Update(token);
            await _ctx.SaveChangesAsync();
        }
    }
}
