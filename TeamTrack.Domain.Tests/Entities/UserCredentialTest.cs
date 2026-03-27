using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class UserCredentialTest
    {
        private static User CreateUser() =>
            User.Create("John", null, "Doe", "jdoe", "john@test.com");

        [Fact]
        public void Create_ShouldFail_WhenUserIsNull()
        {
            Action act = () =>
                UserCredential.Create(null!, "hashed");

            act.Should().Throw<DomainException>()
                .WithMessage("*User cannot be null*");
        }

        [Fact]
        public void Create_ShouldSetValues()
        {
            var user = CreateUser();

            var credential = UserCredential.Create(user, "hashed");

            credential.User.Should().Be(user);
            credential.HashedPassword.Should().Be("hashed");
        }
    }
}