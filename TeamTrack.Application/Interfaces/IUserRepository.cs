using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IUserRepository : IRepository
    {
        Task AddAsync(User user, CancellationToken cancellation);
        
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellation);

        Task<User?> GetByIdAsync(int userId, CancellationToken cancellation);

        void Update(User user);

        Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellation);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellation);
    }
}
