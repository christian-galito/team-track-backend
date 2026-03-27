using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Tests.Services.CurrentUser
{
    public class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserName { get; }

        public string? UserId { get; }

        public IEnumerable<string> Permissions { get; }

        public string? IpAddress { get; }

        public string? UserAgent { get; }

        public FakeCurrentUserService(string? userId = "1", string? userName = "test-user", IEnumerable<string>? permissions = null, string? ipAddress = "test-ip-address", string? userAgent = "test-user-agent")
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
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
