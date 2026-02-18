using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class Role : BaseEntity
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;

        private Role() { }
       
        private Role(string name)
        {
            ValidateAndSetName(name);
        }

        public static Role Create(string name)
        {
            return new Role(name);
        }

        public void ChangeName(string name)
        {
            ValidateAndSetName(name);
        }

        private void ValidateAndSetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Role name cannot be empty.");
            }

            Name = name;
        }

    }
}
