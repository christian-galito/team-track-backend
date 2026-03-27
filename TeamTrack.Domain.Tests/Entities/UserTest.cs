using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class UserTest
    {
        private static User CreateUser()
        {
            return User.Create(
                "John",
                null,
                "Doe",
                "jdoe",
                "john@test.com");
        }

        [Fact]
        public void Create_ShouldCreateUserSuccessfully()
        {
            var user = CreateUser();

            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.UserName.Should().Be("jdoe");
            user.Email.Should().Be("john@test.com");
        }

        [Fact]
        public void Create_ShouldNormalizeEmail()
        {
            var user = User.Create("John", null, "Doe", "jdoe", "JOHN@TEST.COM ");

            user.Email.Should().Be("john@test.com");
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        public void Create_ShouldFail_WhenEmailIsInvalid(string email)
        {
            Action act = () =>
                User.Create("John", null, "Doe", "jdoe", email);

            act.Should().Throw<DomainException>()
                .WithMessage("*Invalid email*");
        }

        [Fact]
        public void Create_ShouldFail_WhenFirstNameIsEmpty()
        {
            Action act = () =>
                User.Create("", null, "Doe", "jdoe", "john@test.com");

            act.Should().Throw<DomainException>()
                .WithMessage("*First name*");
        }

        [Fact]
        public void Create_ShouldFail_WhenLastNameIsEmpty()
        {
            Action act = () =>
                User.Create("John", null, "", "jdoe", "john@test.com");

            act.Should().Throw<DomainException>()
                .WithMessage("*Last name*");
        }

        [Fact]
        public void Create_ShouldFail_WhenUserNameIsEmpty()
        {
            Action act = () =>
                User.Create("John", null, "Doe", "", "john@test.com");

            act.Should().Throw<DomainException>()
                .WithMessage("*Username*");
        }

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
        public void AssignRole_ShouldFail_WhenRoleIdIsInvalid()
        {
            var user = CreateUser();

            Action act = () => user.AssignRole(0);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AssignRole_ShouldNotAddDuplicateRole()
        {
            var user = CreateUser();

            user.AssignRole(1);
            user.AssignRole(1);

            user.Roles.Should().HaveCount(1);
        }

        [Fact]
        public void AddRefreshToken_ShouldAddToken()
        {
            var user = CreateUser();

            user.AddRefreshToken("hashed-token", "127.0.0.1", "agent");

            user.RefreshTokens.Should().ContainSingle();
        }

        [Fact]
        public void AddRefreshToken_ShouldStoreIpAndUserAgent()
        {
            var user = CreateUser();

            user.AddRefreshToken("hashed-token", "127.0.0.1", "agent");

            var token = user.RefreshTokens.First();

            token.IpAddress.Should().Be("127.0.0.1");
            token.UserAgent.Should().Be("agent");
        }

        [Fact]
        public void AddRefreshToken_ShouldFail_WhenTokenIsEmpty()
        {
            var user = CreateUser();

            Action act = () => user.AddRefreshToken("");

            act.Should().Throw<DomainException>();
        }
    }
}
