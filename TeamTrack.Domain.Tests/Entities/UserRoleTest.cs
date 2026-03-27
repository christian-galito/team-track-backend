using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class UserRoleTest
    {
        private static User CreateUser() =>
            User.Create("John", null, "Doe", "jdoe", "john@test.com");

        [Fact]
        public void Create_ShouldFail_WhenUserIsNull()
        {
            Action act = () =>
                UserRole.Create(null!, 1);

            act.Should().Throw<DomainException>()
                .WithMessage("*User cannot be null*");
        }

        [Fact]
        public void Create_ShouldSetValues()
        {
            var user = CreateUser();

            var userRole = UserRole.Create(user, 1);

            userRole.User.Should().Be(user);
            userRole.RoleId.Should().Be(1);
        }
    }
}