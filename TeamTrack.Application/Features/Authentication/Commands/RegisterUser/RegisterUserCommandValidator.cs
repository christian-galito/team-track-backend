using FluentValidation;
using TeamTrack.Application.Common.Extensions;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Authentication.Commands.RegisterUser
{
    public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IUserRepository _userRepository;

        private readonly IRoleRepository _roleRepository;

        public RegisterUserCommandValidator(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;

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

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must have atleast 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role is required.")
                .MustAsync(MustExistRole).WithMessage("Role does not exist.");
        }

        private async Task<bool> MustBeUniqueUserName(string userName, CancellationToken cancellationToken)
        {
            return !await _userRepository.UserNameExistsAsync(userName.NormalizeInput(), cancellationToken);
        }

        private async Task<bool> MustBeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _userRepository.EmailExistsAsync(email.NormalizeInput(true), cancellationToken);
        }

        private async Task<bool> MustExistRole(int roleId, CancellationToken cancellationToken)
        {
            return await _roleRepository.ExistsAsync(roleId, cancellationToken);
        }
    }
}
