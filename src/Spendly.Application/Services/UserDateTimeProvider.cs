using Spendly.Application.Interfaces;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Provides the current date/time converted to a user's local timezone.
    /// Resolves both IANA (e.g. "America/New_York") and Windows (e.g. "Eastern Standard Time")
    /// timezone IDs, falling back to UTC when the ID is null or unrecognized.
    /// </summary>
    public class UserDateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;

        /// <inheritdoc />
        public DateTime Now(string? userTimeZone = null)
        {
            var tz = ResolveTimeZone(userTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        /// <inheritdoc />
        public DateTime Today(string? userTimeZone = null)
            => Now(userTimeZone).Date;

        /// <summary>
        /// Resolves a timezone string to a <see cref="TimeZoneInfo"/> object.
        /// Accepts both IANA and Windows timezone IDs (supported in .NET 6+).
        /// Returns <see cref="TimeZoneInfo.Utc"/> if the ID is null, empty, or unknown.
        /// </summary>
        public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return TimeZoneInfo.Utc;

            if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz))
                return tz;

            // Fallback: UTC
            return TimeZoneInfo.Utc;
        }
    }
}
