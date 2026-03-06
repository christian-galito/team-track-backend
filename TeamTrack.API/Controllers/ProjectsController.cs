using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeamTrack.Application.Features.Projects.Commands.CreateProject;
using TeamTrack.Application.Features.Projects.Commands.UpdateProject;
using TeamTrack.Application.Features.Projects.Commands.DeleteProject;
using TeamTrack.Application.Features.Projects.Queries;

namespace TeamTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetProjectById(int projectId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProjectByIdQuery(projectId), cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProjectsQuery(), cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetProjectById), new { projectId = result.ProjectId }, result);
        }

        [HttpPut("{projectId:int}")]
        public async Task<IActionResult> UpdateProject(int projectId, [FromBody] UpdateProjectCommand command, CancellationToken cancellationToken)
        {
            command = command with { ProjectId = projectId };
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{projectId:int}")]
        public async Task<IActionResult> DeleteProject(int projectId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteProjectCommand(projectId), cancellationToken);

            return NoContent();
        }
    }
}

