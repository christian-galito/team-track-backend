namespace TeamTrack.Application.Interfaces
{
    public interface IRoleRepository : IRepository
    {
        Task<bool> ExistsAsync(int roleId , CancellationToken cancellationToken);
    }
}
