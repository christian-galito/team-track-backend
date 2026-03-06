using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Projects.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Projects.Commands.UpdateProject
{
    public record UpdateProjectCommand(int ProjectId, string Name) : IRequest<ProjectResponseDto>
    {
        public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectResponseDto>
        {
            private readonly IProjectRepository _projectRepository;
            private readonly IUnitOfWork _unitOfWork;

            public UpdateProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
            {
                _projectRepository = projectRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<ProjectResponseDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

                if (project is null)
                {
                    throw new NotFoundException(nameof(Project), request.ProjectId);
                }

                project.ChangeName(request.Name);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ProjectResponseDto(
                    ProjectId: project.Id,
                    Name: project.Name);
            }
        }
    }
}

