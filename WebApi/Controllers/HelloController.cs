
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class HelloController : ControllerBase
    {
        private readonly ILogger<HelloController> _logger;
        public HelloController(ILogger<HelloController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/hello")]
        [HasClaim("sub")]
        [HasClaim("userFart")]  //TODO:  this could also be replaced by a policy... I will try after fixing Clerk in the frontend
        public ActionResult<string> ClownsWorld()
        {
            _logger.LogInformation("ClownsWorld endpoint called");

            var userId = HttpContext?.User.FindFirst("sub")?.Value;
            var userFart = HttpContext?.User.FindFirst("userFart")?.Value;

            return Ok($"Hello, {userFart}! (UserID: {userId})");
        }
    }
}