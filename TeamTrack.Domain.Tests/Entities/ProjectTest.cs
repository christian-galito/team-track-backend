using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class ProjectTest
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenNameIsEmpty()
        {
            Action act = () => new Project(string.Empty);

            act.Should().Throw<DomainException>()
                .WithMessage("*Project name cannot be empty*");
        }

        [Fact]
        public void ChangeName_ShouldUpdateName_WhenValid()
        {
            var project = new Project("Initial");

            project.ChangeName("Updated");

            project.Name.Should().Be("Updated");
        }
    }
}

