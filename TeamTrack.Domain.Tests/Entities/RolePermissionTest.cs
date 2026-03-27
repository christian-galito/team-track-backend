using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class RolePermissionTest
    {
        [Fact]
        public void Create_ShouldFail_WhenRoleIsNull()
        {
            Action act = () =>
                RolePermission.Create(null!, 1);

            act.Should().Throw<DomainException>()
                .WithMessage("*Role cannot be null*");
        }

        [Fact]
        public void Create_ShouldSetValues()
        {
            var role = new Role("Admin");

            var rp = RolePermission.Create(role, 1);

            rp.Role.Should().Be(role);
            rp.PermissionId.Should().Be(1);
        }
    }
}