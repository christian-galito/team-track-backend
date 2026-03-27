using FluentAssertions;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Domain.Tests.Entities
{
    public class PermissionTest
    {
        [Fact]
        public void Constructor_ShouldFail_WhenNameIsEmpty()
        {
            Action act = () => new Permission("", null);

            act.Should().Throw<DomainException>()
                .WithMessage("*Permission name cannot be null*");
        }

        [Fact]
        public void Constructor_ShouldTrimName()
        {
            var permission = new Permission("  ReadUser  ", null);

            permission.Name.Should().Be("ReadUser");
        }
    }
}