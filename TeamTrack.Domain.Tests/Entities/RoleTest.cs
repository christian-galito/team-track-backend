using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class RoleTest
    {
        [Fact]
        public void Constructor_ShouldFail_WhenNameIsEmpty()
        {
            Action act = () => new Role("");

            act.Should().Throw<DomainException>()
                .WithMessage("*Role name cannot be empty*");
        }

        [Fact]
        public void AddPermission_ShouldAddPermission()
        {
            var role = new Role("Admin");

            role.AddPermission(1);

            role.Permissions.Should().ContainSingle(p => p.PermissionId == 1);
        }

        [Fact]
        public void AddPermission_ShouldNotAddDuplicate()
        {
            var role = new Role("Admin");

            role.AddPermission(1);
            role.AddPermission(1);

            role.Permissions.Should().HaveCount(1);
        }

        [Fact]
        public void AddPermission_ShouldFail_WhenIdIsInvalid()
        {
            var role = new Role("Admin");

            Action act = () => role.AddPermission(0);

            act.Should().Throw<DomainException>();
        }
    }
}