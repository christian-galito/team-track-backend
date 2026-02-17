using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Tests.Builders
{
    public class UserBuilder
    {
        private string _userName = "jdoe";
        private string _email = "john@test.com";
        private string _firstName = "John";
        private string _lastName = "Doe";
        private string? _middleName;
        private int? _roleId;
        private string _hashedPassword = "hashed-password";

        public UserBuilder WithName(string firstName, string? middleName, string lastName)
        {
            _firstName = firstName;
            _middleName = middleName;
            _lastName = lastName;
            return this;
        }

        public UserBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithRole(int roleId)
        {
            _roleId = roleId;
            return this;
        }

        public UserBuilder WithPassword(string hashedPassword)
        {
            _hashedPassword = hashedPassword;
            return this;
        }

        public User Build()
        {
            var user = User.Register(
                firstName: _firstName,
                middleName: _middleName,
                lastName: _lastName,
                userName: _userName,
                email: _email,
                hashedPassword: _hashedPassword
            );

            if (_roleId.HasValue)
            {
                user.AssignRole(_roleId.Value);
            }

            return user;
        }
    }
}
