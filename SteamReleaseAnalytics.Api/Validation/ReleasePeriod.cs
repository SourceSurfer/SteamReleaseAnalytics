using System.Globalization;

namespace SteamReleaseAnalytics.Api.Validation
{
    /// <summary>
    /// Допустимый период для запросов по месяцу релиза.
    /// </summary>
    internal static class ReleasePeriod
    {
        public const int MinYear = 1970;
        public const int MaxYear = 2100;

        public static string YearRangeMessage => $"Год должен быть от {MinYear} до {MaxYear}";

        public static bool IsSupportedYear(int year) => year is >= MinYear and <= MaxYear;

        /// <summary>
        /// Разбирает месяц строго в формате yyyy-MM, независимо от культуры сервера.
        /// </summary>
        public static bool TryParseMonth(string? value, out DateOnly month) =>
            DateOnly.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out month)
            && IsSupportedYear(month.Year);
    }
}
