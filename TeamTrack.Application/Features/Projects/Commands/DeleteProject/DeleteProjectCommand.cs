using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Projects.Commands.DeleteProject
{
    public record DeleteProjectCommand(int ProjectId) : IRequest
    {
        public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
        {
            private readonly IProjectRepository _projectRepository;
            private readonly IUnitOfWork _unitOfWork;

            public DeleteProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
            {
                _projectRepository = projectRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

                if (project is null)
                {
                    throw new NotFoundException(nameof(Project), request.ProjectId);
                }

                _projectRepository.Delete(project);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

