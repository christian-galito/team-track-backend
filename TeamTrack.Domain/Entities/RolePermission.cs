using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        public int RoleId { get; private set; }

        public int PermissionId { get; private set; }

        public virtual Role Role { get; set; } = null!;

        public virtual Permission Permission { get; set; } = null!;

        private RolePermission() { }

        public RolePermission(Role role, int permissionId)
        {
            ValidateAndSetRolePermission(role, permissionId);
        }

        private void ValidateAndSetRolePermission(Role role, int permissionId)
        {
            if (role == null)
            {
                throw new DomainException("Role cannot be null.");
            }

            Role = role;
            PermissionId = permissionId;
        }

        internal static RolePermission Create(Role role, int permissionId)
        {
            return new RolePermission(role, permissionId);
        }
    }
}
