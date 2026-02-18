using TeamTrack.Domain.Common;

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
                throw new DomainException("User cannot be null.");
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
