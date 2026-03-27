namespace TeamTrack.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? IpAddress { get; }

        string? UserAgent { get; }

        string? UserId { get; }

        string? UserName { get; }

        IEnumerable<string> Permissions { get; }
    }
}
