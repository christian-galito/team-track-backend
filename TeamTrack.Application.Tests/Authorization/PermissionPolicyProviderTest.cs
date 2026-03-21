using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TeamTrack.Application.Authorization;
using TeamTrack.Domain.Security;

namespace TeamTrack.Application.Tests.Authorization
{
    public class PermissionPolicyProviderTest
    {
        private readonly IOptions<AuthorizationOptions> _options;

        public PermissionPolicyProviderTest()
        {
            _options = Options.Create(new AuthorizationOptions());
        }
        [Fact]
        public async Task GetPolicyAsync_ShouldReturnPolicy_WhenPermissionNameIsValid()
        {
            var provider = new PermissionPolicyProvider(_options);
            var policyName = Permissions.User.Create.Name;

            var policy = await provider.GetPolicyAsync(policyName);

            policy.Should().NotBeNull();
            policy!.Requirements.Should().ContainSingle(r => r is PermissionRequirement);
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldThrow_WhenPermissionNameIsUnknown()
        {
            var provider = new PermissionPolicyProvider(_options);
            var invalidName = "NonExistentPermission";

            Func<Task> act = () => provider.GetPolicyAsync(invalidName);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
