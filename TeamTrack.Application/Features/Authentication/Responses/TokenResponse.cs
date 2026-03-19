namespace TeamTrack.Application.Features.Authentication.Responses
{
    public sealed record TokenResponse(string AccessToken, string RefreshToken);
}
