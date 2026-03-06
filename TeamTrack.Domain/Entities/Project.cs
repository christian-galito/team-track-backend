using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class Project : BaseEntity
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;

        private Project()
        {
        }

        public Project(string name)
        {
            ValidateAndSetName(name);
        }

        public void ChangeName(string name)
        {
            ValidateAndSetName(name);
        }

        private void ValidateAndSetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Project name cannot be empty.");
            }

            Name = name.Trim();
        }
    }
}

