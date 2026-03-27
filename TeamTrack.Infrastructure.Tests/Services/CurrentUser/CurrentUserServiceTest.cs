using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Security.Claims;
using TeamTrack.Infrastructure.Services.CurrentUser;

namespace TeamTrack.Infrastructure.Tests.Services.CurrentUser
{
    public class CurrentUserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

        private CurrentUserService CreateService(HttpContext context)
        {
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(context);

            return new CurrentUserService(_httpContextAccessorMock.Object);
        }

        [Fact]
        public void IpAddress_ShouldReturnNull_WhenNoHttpContext()
        {
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext?)null);

            var service = new CurrentUserService(_httpContextAccessorMock.Object);

            service.IpAddress.Should().BeNull();
        }

        [Fact]
        public void IpAddress_ShouldMapIPv6ToIPv4()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("::ffff:192.168.1.1");

            var service = CreateService(context);

            service.IpAddress.Should().Be("192.168.1.1");
        }

        [Fact]
        public void IpAddress_ShouldReturnIPv4Address()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.10");

            var service = CreateService(context);

            service.IpAddress.Should().Be("192.168.1.10");
        }

        [Fact]
        public void UserAgent_ShouldReturnHeaderValue()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "test-agent";

            var service = CreateService(context);

            service.UserAgent.Should().Be("test-agent");
        }

        [Fact]
        public void UserAgent_ShouldReturnNull_WhenHeaderMissing()
        {
            var context = new DefaultHttpContext();

            var service = CreateService(context);

            service.UserAgent.Should().BeNullOrEmpty();
        }

        [Fact]
        public void UserId_ShouldReturnClaimValue()
        {
            var context = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            };

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var service = CreateService(context);

            service.UserId.Should().Be("123");
        }

        [Fact]
        public void UserId_ShouldReturnZero_WhenClaimMissing()
        {
            var context = new DefaultHttpContext();

            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            var service = CreateService(context);

            service.UserId.Should().Be("0");
        }

        [Fact]
        public void UserName_ShouldReturnClaimValue()
        {
            var context = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim("username", "johndoe")
            };

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var service = CreateService(context);

            service.UserName.Should().Be("johndoe");
        }

        [Fact]
        public void UserName_ShouldReturnDefault_WhenMissing()
        {
            var context = new DefaultHttpContext();

            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            var service = CreateService(context);

            service.UserName.Should().Be("System User");
        }

        [Fact]
        public void Permissions_ShouldReturnAllPermissionClaims()
        {
            var context = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim("Permission", "Read"),
                new Claim("Permission", "Write")
            };

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var service = CreateService(context);

            service.Permissions.Should().BeEquivalentTo(new[] { "Read", "Write" });
        }

        [Fact]
        public void Permissions_ShouldReturnEmpty_WhenNoClaims()
        {
            var context = new DefaultHttpContext();

            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            var service = CreateService(context);

            service.Permissions.Should().BeEmpty();
        }
    }
}