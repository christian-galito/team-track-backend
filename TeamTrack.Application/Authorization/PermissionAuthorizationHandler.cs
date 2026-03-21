using Microsoft.AspNetCore.Authorization;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Authorization
{
    public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ICurrentUserService _currentUser;

        public PermissionAuthorizationHandler(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (_currentUser.Permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
