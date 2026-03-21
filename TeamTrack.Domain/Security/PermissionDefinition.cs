namespace TeamTrack.Domain.Security
{
    public sealed class PermissionDefinition
    {
        public int Id { get; init; }

        public string Name { get; init; } = default!;

        public string? Description {  get; init; } 
    }
}
