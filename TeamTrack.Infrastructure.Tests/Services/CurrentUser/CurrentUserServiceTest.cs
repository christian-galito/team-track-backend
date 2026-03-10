using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Tests.Services.CurrentUser
{
    public class CurrentUserServiceTest : ICurrentUserService
    {
        public string? UserName { get; }

        public string? UserId { get; }

        public CurrentUserServiceTest(string? userId = "1", string? userName = "test-user")
        {
            UserName = userName;
            UserId = userId;
        }
    }
}
