namespace TeamTrack.Domain.Entities
{
    internal class AuthToken
    {
        public string Token { get; }

        public DateTime Expiration { get; }

        public AuthToken(string token, DateTime expiration)
        {
            Token = token;
            Expiration = expiration;
        }
    }
}
