using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Tests.Services.CurrentUser
{
    public class CurrentUserServiceTest : ICurrentUserService
    {
        public string? UserName { get; }

        public CurrentUserServiceTest(string? userName = "test-user")
        {
            UserName = userName;
        }
    }
}
