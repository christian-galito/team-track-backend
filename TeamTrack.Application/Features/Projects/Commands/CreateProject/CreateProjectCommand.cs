using MediatR;
using TeamTrack.Application.Features.Projects.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Projects.Commands.CreateProject
{
    public record CreateProjectCommand(string Name) : IRequest<ProjectResponseDto>
    {
        public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponseDto>
        {
            private readonly IProjectRepository _projectRepository;
            private readonly IUnitOfWork _unitOfWork;

            public CreateProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
            {
                _projectRepository = projectRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<ProjectResponseDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
            {
                var project = new Project(request.Name);

                await _projectRepository.AddAsync(project, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ProjectResponseDto(
                    ProjectId: project.Id,
                    Name: project.Name);
            }
        }
    }
}

