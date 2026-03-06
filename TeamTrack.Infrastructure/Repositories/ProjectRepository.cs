using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Interfaces;

namespace TeamTrack.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ITeamTrackDbContext _context;

        public ProjectRepository(ITeamTrackDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Project project, CancellationToken cancellationToken)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            await _context.Projects.AddAsync(project, cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetAsync(CancellationToken cancellationToken)
        {
            return await _context.Projects
                .ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(int projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        }

        public void Update(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            _context.Projects.Update(project);
        }

        public void Delete(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            project.MarkAsDeleted();
        }

        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Projects.AnyAsync(p => p.Name == name, cancellationToken);
        }

        public async Task<bool> NameExistsAsync(string name, int excludedProjectId, CancellationToken cancellationToken)
        {
            return await _context.Projects.AnyAsync(p => p.Name == name && p.Id != excludedProjectId, cancellationToken);
        }
    }
}

