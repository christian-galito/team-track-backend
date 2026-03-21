using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;

        public string? Description { get; private set; }

        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

        private Permission() { }

        public Permission(string name, string? description)
        {
            ValidateAndSetName(name);
            Description = description;
        }

        private void ValidateAndSetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Permission name cannot be null.");
            }

            Name = name.Trim();
        }
    }
}
