using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IUserRepository : IRepository
    {
        Task AddAsync(User user, CancellationToken cancellationToken);

        void Delete(User user);
        
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

        Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken);

        void Update(User user);

        Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken);

        Task<bool> UserNameExistsAsync(string userName, int excludedUserId, CancellationToken cancellationToken);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

        Task<bool> EmailExistsAsync(string email, int excludedUserId, CancellationToken cancellationToken);
    }
}
