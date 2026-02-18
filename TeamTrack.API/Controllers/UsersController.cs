using Microsoft.AspNetCore.Mvc;

namespace TeamTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public IActionResult GetUserById(int userId)
        {
            // TODO: 
            return Ok();
        }
    }
}
