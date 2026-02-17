namespace TeamTrack.Domain.Entities
{
    public class UserRole 
    {
        public int UserId { get; private set; }

        public virtual User User { get; set; } = null!;

        public int RoleId { get; set; }

        public virtual Role Role { get; set; } = null!;

        private UserRole() { }

        private UserRole(User user, int roleId)
        {
           ValidateAndSetUserRole(user, roleId);
        }

        private void ValidateAndSetUserRole(User user, int roleId)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (roleId <= 0)
            {
                throw new ArgumentException("Role ID must be a positive integer.", nameof(roleId));
            }

            User = user;
            RoleId = roleId;
        }
        internal static UserRole Create(User user, int roleId)
        {
            return new UserRole(user, roleId);
        }
    }
}
