namespace Spendly.Application.Interfaces
{
    /// <summary>
    /// Abstraction for obtaining the current date/time in the user's local timezone.
    /// All application logic should use this interface instead of DateTime.UtcNow
    /// to correctly handle users in timezones that differ from UTC.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Returns the current date and time converted to the user's local timezone.
        /// Falls back to UTC if the timezone string is null or unrecognized.
        /// </summary>
        DateTime Now(string? userTimeZone = null);

        /// <summary>
        /// Returns today's date (no time component) in the user's local timezone.
        /// Falls back to UTC if the timezone string is null or unrecognized.
        /// </summary>
        DateTime Today(string? userTimeZone = null);

        /// <summary>
        /// Returns the current UTC datetime (for audit/log purposes only).
        /// Do NOT use this for business logic involving the user's "today".
        /// </summary>
        DateTime UtcNow { get; }
    }
}
