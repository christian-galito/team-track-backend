using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Interfaces;

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

        public void Delete(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.MarkAsDeleted();
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, int excludedUserId, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludedUserId, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellation)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Credentials)
                .FirstOrDefaultAsync(u => u.Email == email, cancellation);
        }

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Credentials)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken)
        {
            var userRoles = await _context.UserRoles
               .Where(u => u.UserId == userId)
               .Include(u => u.Role.Permissions)
                   .ThenInclude(r => r.Permission)
               .ToListAsync(cancellationToken);

            return userRoles
                .SelectMany(ur => ur.Role.Permissions)
                .Select(p => p.Permission.Name)
                .Distinct();
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

        public async Task<bool> UserNameExistsAsync(string userName, int excludedUserId, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName && u.Id != excludedUserId, cancellationToken);
        }
    }
}
