namespace TeamTrack.Application.Common.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeInput(this string? value, bool toLower = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim();
            return toLower ? normalized.ToLowerInvariant() : normalized;
        }
    }
}