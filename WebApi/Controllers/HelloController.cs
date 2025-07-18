
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
        public ActionResult<string> ClownsWorld()
        {
            _logger.LogInformation("ClownsWorld endpoint called");

            var userId = HttpContext?.User.FindFirst("sub")?.Value;

            return Ok($"Hello,  (UserID: {userId})");
        }
    }
}