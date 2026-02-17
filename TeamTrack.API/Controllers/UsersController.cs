using Microsoft.AspNetCore.Mvc;

namespace TeamTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id:int}")]
        public IActionResult GetByUserId(int userId)
        {
            // TODO: 
            return Ok();
        }
    }
}
