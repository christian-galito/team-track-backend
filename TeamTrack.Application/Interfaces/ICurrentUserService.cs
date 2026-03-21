namespace TeamTrack.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        string? UserName { get; }

        IEnumerable<string> Permissions { get; }
    }
}
