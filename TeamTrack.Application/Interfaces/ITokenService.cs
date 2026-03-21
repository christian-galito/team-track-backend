namespace TeamTrack.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(int userId, string username, string email, CancellationToken cancellationToken);

        string GenerateRefreshToken();
    }
}
