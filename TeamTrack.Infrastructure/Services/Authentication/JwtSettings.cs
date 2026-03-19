namespace TeamTrack.Infrastructure.Services.Authentication
{
    public class JwtSettings
    {
        public string Issuer { get; init; } = default!;

        public string Audience { get; init; } = default!;

        public string SecretKey { get; init; } = default!;

        public string RefreshTokenHashKey { get ; init; } = default!;

        public int ExpirationMinutes { get; init; }
    }
}
