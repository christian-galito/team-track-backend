using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;
using TeamTrack.Application.Authorization;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Security;

namespace TeamTrack.Application.Tests.Authorization
{
    public class PermissionAuthorizationHandlerTest
    {
        private readonly Mock<ICurrentUserService> _currentUserMock = new();

        public PermissionAuthorizationHandlerTest()
        {
            _currentUserMock.Setup(x => x.Permissions).Returns(Permissions.All.Select(x => x.Name));
        }

        [Fact]
        public async Task Handler_ShouldSucceed_WhenCurrentUserHasRequiredPermission()
        {
            var requirement = new PermissionRequirement(Permissions.User.Create.Name);
            var handler = new PermissionAuthorizationHandler(_currentUserMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("permission", Permissions.User.Create.Name)
            }));

            var context = new AuthorizationHandlerContext(
                new[] { requirement }, user, null);

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task Handler_ShouldFail_WhenCurrentUserLacksRequiredPermission()
        {
            _currentUserMock
                .Setup(x => x.Permissions)
                .Returns(Array.Empty<string>());

            var requirement = new PermissionRequirement(Permissions.User.Create.Name);
            var handler = new PermissionAuthorizationHandler(_currentUserMock.Object);

            var user = new ClaimsPrincipal();

            var context = new AuthorizationHandlerContext(
                new[] { requirement }, user, null);

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }
    }
}
