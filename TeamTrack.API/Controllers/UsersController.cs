using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeamTrack.Application.Features.Users.Commands.DeleteUser;
using TeamTrack.Application.Features.Users.Commands.UpdateUser;
using TeamTrack.Application.Features.Users.Queries;

namespace TeamTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(userId), cancellationToken);

            return Ok(result);
        }

        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
        {
            command = command with { UserId = userId };
            var result =  await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);
            
            return NoContent();
        }
    }
}
