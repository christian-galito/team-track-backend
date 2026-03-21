using Microsoft.AspNetCore.Authorization;

namespace TeamTrack.Application.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}