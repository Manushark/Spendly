using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}