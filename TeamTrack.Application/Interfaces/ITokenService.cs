namespace TeamTrack.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string username, string email);

        string GenerateRefreshToken();
    }
}
