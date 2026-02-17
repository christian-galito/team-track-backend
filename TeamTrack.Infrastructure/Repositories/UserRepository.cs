using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ITeamTrackDbContext _context;
        public UserRepository(ITeamTrackDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _context.Users.AddAsync(user, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Credentials)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public void Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Users.Update(user);
        }

        public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
        }
    }
}
