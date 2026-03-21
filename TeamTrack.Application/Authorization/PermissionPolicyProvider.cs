using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TeamTrack.Domain.Security;

namespace TeamTrack.Application.Authorization
{
    public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        private static readonly HashSet<string> _allPermissionNames = Permissions.All
          .Select(p => p.Name)
          .ToHashSet();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!_allPermissionNames.Contains(policyName))
            {
                throw new InvalidOperationException($"Unknown permission policy: {policyName}");
            }

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}
