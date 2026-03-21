using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Tests.Services.CurrentUser
{
    public class CurrentUserServiceTest : ICurrentUserService
    {
        public string? UserName { get; }

        public string? UserId { get; }

        public IEnumerable<string> Permissions { get; }

        public CurrentUserServiceTest(string? userId = "1", string? userName = "test-user", IEnumerable<string>? permissions = null)
        {
            UserName = userName;
            UserId = userId;
            Permissions = permissions ?? new[]
            {
                "CreateUser",
                "ReadUser",
                "UpdateUser",
                "DeleteUser"
            };
        }
    }
}
