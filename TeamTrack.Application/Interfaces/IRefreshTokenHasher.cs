namespace TeamTrack.Application.Interfaces
{
    public interface IRefreshTokenHasher
    {
        public string HashRefreshToken(string refreshToken);
    }
}
