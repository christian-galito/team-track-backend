using FluentValidation;

namespace TeamTrack.Application.Features.Authentication.Commands.TokenRefresh
{
    public class TokenRefreshCommandValidator : AbstractValidator<TokenRefreshCommand>
    {
        public TokenRefreshCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");
        }
    }
}
