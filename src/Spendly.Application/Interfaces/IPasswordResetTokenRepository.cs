using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task UpdateAsync(PasswordResetToken token);
    }
}
