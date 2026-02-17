using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class UserCredential : BaseEntity
    {
        public int Id { get; private set; }

        public string HashedPassword { get; private set; } = null!;

        public int UserId { get; private set; }

        public User User { get; private set; } = null!;

        public bool IsActive { get; private set; }

        private UserCredential() { }

        private UserCredential(User user, string hashedPassword)
        {
            ValidateAndSetCredentials(user, hashedPassword);
        }

        private void ValidateAndSetCredentials(User user, string hashedPassword)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new ArgumentException("Hashed password cannot be empty.", nameof(hashedPassword));
            }

            User = user;
            HashedPassword = hashedPassword;
        }

        internal static UserCredential Create(User user, string hashedPassword)
        {
            return new UserCredential(user, hashedPassword);
        }
    }
}
