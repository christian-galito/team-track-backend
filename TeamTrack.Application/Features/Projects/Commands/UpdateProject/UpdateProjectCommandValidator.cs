using FluentValidation;
using TeamTrack.Application.Common.Extensions;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Projects.Commands.UpdateProject
{
    public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;

        public UpdateProjectCommandValidator(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.")
                .MustAsync(MustBeUniqueName).WithMessage("Project name already exists.");
        }

        private async Task<bool> MustBeUniqueName(UpdateProjectCommand command, string name, CancellationToken cancellationToken)
        {
            var normalized = name.NormalizeInput();
            return !await _projectRepository.NameExistsAsync(normalized, command.ProjectId, cancellationToken);
        }
    }
}

