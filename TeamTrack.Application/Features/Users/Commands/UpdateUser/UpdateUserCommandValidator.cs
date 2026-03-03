using FluentValidation;
using TeamTrack.Application.Common.Extensions;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        public UpdateUserCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.MiddleName)
                .MaximumLength(100).WithMessage("Middle name cannot exceed 100 characters.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required.")
                .MaximumLength(50).WithMessage("User name cannot exceed 50 characters.")
                .MustAsync(MustBeUniqueUserName).WithMessage("User name already exists.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MustAsync(MustBeUniqueEmail).WithMessage("Email already exists.");
        }

        private async Task<bool> MustBeUniqueUserName(UpdateUserCommand command, string userName, CancellationToken cancellationToken)
        {
            return !await _userRepository.UserNameExistsAsync(userName.NormalizeInput(), command.UserId, cancellationToken);
        }

        private async Task<bool> MustBeUniqueEmail(UpdateUserCommand command, string email, CancellationToken cancellationToken)
        {
            return !await _userRepository.EmailExistsAsync(email.NormalizeInput(true), command.UserId, cancellationToken);
        }
    }
}
