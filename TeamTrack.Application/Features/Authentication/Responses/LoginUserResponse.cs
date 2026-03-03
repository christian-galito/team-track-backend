namespace TeamTrack.Application.Features.Authentication.Responses
{
    public record LoginUserResponse
    {
        public int UserId { get; init; }

        public string UserName { get; init; }

        public LoginUserResponse(int userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

    }
}
