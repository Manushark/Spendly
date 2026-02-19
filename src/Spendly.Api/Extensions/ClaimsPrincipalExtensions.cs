using System.Security.Claims;
using Spendly.Domain.Exceptions;

namespace Spendly.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extrae el UserId del token JWT. Lanza excepción si no está autenticado.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("sub");

            if (claim is null || !int.TryParse(claim.Value, out var userId))
                throw new InvalidDomainException("User identity could not be determined.");

            return userId;
        }
    }
}