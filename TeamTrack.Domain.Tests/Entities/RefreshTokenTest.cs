using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class RefreshTokenTest
    {
        private static User CreateUser() =>
            User.Create(
                firstName:"John", 
                middleName: null, 
                lastName: "Doe", 
                userName: "jdoe", 
                email: "john@test.com");

        [Fact]
        public void Create_ShouldFail_WhenUserIsNull()
        {
            Action act = () =>
                RefreshToken.Create(null!, "token");

            act.Should().Throw<DomainException>()
                .WithMessage("*User cannot be null*");
        }

        [Fact]
        public void Create_ShouldFail_WhenTokenIsEmpty()
        {
            var user = CreateUser();

            Action act = () =>
                RefreshToken.Create(user, "");

            act.Should().Throw<DomainException>()
                .WithMessage("*token*");
        }

        [Fact]
        public void Create_ShouldBeActive_AfterCreation()
        {
            var user = CreateUser();

            var token = RefreshToken.Create(user, "token");

            token.IsActive().Should().BeTrue();
        }

        [Fact]
        public void IsActive_ShouldReturnFalse_WhenRevoked()
        {
            var user = CreateUser();
            var token = RefreshToken.Create(user, "token");

            token.Revoke();

            token.IsActive().Should().BeFalse();
        }

        [Fact]
        public void IsActive_ShouldReturnFalse_WhenExpired()
        {
            var user = CreateUser();

            var token = new RefreshToken(
                user,
                "token",
                DateTime.UtcNow.AddMinutes(-1));

            token.IsActive().Should().BeFalse();
        }

        [Fact]
        public void Revoke_ShouldSetPropertiesCorrectly()
        {
            var user = CreateUser();
            var token = RefreshToken.Create(user, "token");

            token.Revoke("new-token", "test-reason");

            token.IsRevoked.Should().BeTrue();
            token.ReplacedByToken.Should().Be("new-token");
            token.RevokedBy.Should().Be("test-reason");
            token.RevokedAt.Should().NotBeNull();
        }
    }
}