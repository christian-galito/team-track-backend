using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Register_ShouldFail_WhenNoPasswordIsAdded()
        {
            Action act = () =>
            {
                var user = User.Register(
                    firstName: "John",
                    middleName: null,
                    lastName: "Doe",
                    userName: "jdoe",
                    email: "john@test.com",
                    hashedPassword: ""
                );
            };

            act.Should().Throw<DomainException>()
                .WithMessage("*credential*");
        }

        [Fact]
        public void Register_ShouldCreateUser_WithCredential()
        {
            var user = User.Register(
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "jdoe",
                email: "john@test.com",
                hashedPassword: "hashed-password"
            );

            user.Credentials.Should().ContainSingle();
        }

        [Fact]
        public void AssignRole_ShouldNotAddDuplicateRole()
        {
            var user = User.Register(
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "jdoe",
                email: "john@test.com",
                hashedPassword: "hashed-password"
            );

            user.AssignRole(1);
            user.AssignRole(1);

            user.Roles.Should().HaveCount(1);
        }
    }
}
