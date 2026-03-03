using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class User : BaseEntity
    {
        private readonly List<UserRole> _roles = new();

        private readonly List<UserCredential> _credentials = new();

        public int Id { get; private set; }

        public string FirstName { get; private set; } = null!;

        public string LastName { get; private set; } = null!;

        public string? MiddleName { get; private set; }

        public string Email { get; private set; } = null!;

        public string UserName { get; private set; } = null!;

        public IReadOnlyCollection<UserRole> Roles => _roles;

        public IReadOnlyCollection<UserCredential> Credentials => _credentials;

        private User() { }

        public User(string email, string userName, string firstName, string lastName, string? middleName = null)
        {
            ValidateAndSetName(firstName, lastName, middleName);
            ValidateAndSetEmail(email);
            ValidateAndSetUserName(userName);
        }

        public static User Register(string firstName, string? middleName, string lastName, string userName, string email, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new DomainException("User must have atleast one credential.");
            }
            var user = new User(email, userName, firstName, lastName, middleName);

            user.AddCredential(hashedPassword);

            return user;
        }

        public void ChangeName(string firstName, string lastName, string? middleName)
        {
            ValidateAndSetName(firstName, lastName, middleName);
        }

        public void ChangeEmail(string email)
        {
            ValidateAndSetEmail(email);
        }

        public void ChangeUserName(string userName)
        {
            ValidateAndSetUserName(userName);
        }

        private void ValidateAndSetName(string firstName, string lastName, string? middleName = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new DomainException("First name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new DomainException("Last name cannot be empty.");
            }

            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }

        private void ValidateAndSetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                throw new DomainException("Invalid email address.");
            }

            Email = email.Trim().ToLowerInvariant();
        }

        private void ValidateAndSetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new DomainException("Username cannot be empty.");
            }

            UserName = userName.Trim();
        }

        public void AssignRole(int roleId)
        {
            if (roleId <= 0)
            {
                throw new DomainException("Role ID must be a positive integer.");
            }
            
            if (_roles.Any(r => r.RoleId == roleId))
            {
                return;
            }

            _roles.Add(UserRole.Create(this, roleId));
        }

        public void AddCredential(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new DomainException("Credential cannot be empty.");
            }

            _credentials.Add(UserCredential.Create(this, hashedPassword));
        }
    }
}
