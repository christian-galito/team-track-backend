using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Projects.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Projects.Queries
{
    public record GetProjectByIdQuery(int ProjectId) : IRequest<ProjectResponseDto>
    {
        public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponseDto>
        {
            private readonly IProjectRepository _projectRepository;

            public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
            {
                _projectRepository = projectRepository;
            }

            public async Task<ProjectResponseDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

                if (project is null)
                {
                    throw new NotFoundException(nameof(Project), request.ProjectId);
                }

                return new ProjectResponseDto(
                    ProjectId: project.Id,
                    Name: project.Name);
            }
        }
    }
}

