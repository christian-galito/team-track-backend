using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IProjectRepository : IRepository
    {
        Task AddAsync(Project project, CancellationToken cancellationToken);

        Task <IEnumerable<Project>> GetAsync(CancellationToken cancellationToken);

        Task<Project?> GetByIdAsync(int projectId, CancellationToken cancellationToken);

        void Update(Project project);

        void Delete(Project project);

        Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken);

        Task<bool> NameExistsAsync(string name, int excludedProjectId, CancellationToken cancellationToken);
    }
}

