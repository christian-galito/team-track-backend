using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeamTrack.Application.Features.Authentication.Command.RegisterUser;

namespace TeamTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                actionName: "GetUserById",
                controllerName: "Users",
                routeValues: new { userId = result.UserId },
                value: result);
        }

    }
}
