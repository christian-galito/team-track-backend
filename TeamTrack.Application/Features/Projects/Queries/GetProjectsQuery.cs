using MediatR;
using TeamTrack.Application.Features.Projects.Responses;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Projects.Queries
{
    public class GetProjectsQuery : IRequest<List<ProjectResponseDto>>
    {
        public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectResponseDto>>
        {
            private readonly IProjectRepository _projectRepository;

            public GetProjectsQueryHandler(IProjectRepository projectRepository)
            {
                _projectRepository = projectRepository;
            }

            public async Task<List<ProjectResponseDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
            {
                var projects = await _projectRepository.GetAsync(cancellationToken);

                return projects.Select(p => new ProjectResponseDto(
                    ProjectId: p.Id,
                    Name: p.Name)).ToList();
            }
        }
    }
}
