using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class Role : BaseEntity
    {
        private readonly List<RolePermission> _permissions = new();

        public int Id { get; private set; }

        public string Name { get; private set; } = null!;

        public string? Description { get; private set; }

        public IReadOnlyCollection<RolePermission> Permissions => _permissions;

        private Role() { }
       
        public Role(string name, string? description = null)
        {
            ValidateAndSetName(name);
            Description = description;
        }

        public void ChangeName(string name)
        {
            ValidateAndSetName(name);
        }
        
        public void ChangeDescription(string? description)
        {
            Description = description;
        }

        private void ValidateAndSetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Role name cannot be empty.");
            }

            Name = name;
        }

        public void AddPermission(int permissionId)
        {
            if (permissionId <= 0)
            {
                throw new DomainException("Permission ID must be a positive integer.");
            }

            if (_permissions.Any(p => p.PermissionId == permissionId))
            {
                return;
            }

            _permissions.Add(RolePermission.Create(this, permissionId));
        }
    }
}
