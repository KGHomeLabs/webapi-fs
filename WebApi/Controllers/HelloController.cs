
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    private readonly IUserDataService _userDataService; 

    public HelloController(IUserDataService userDataService)
    {
        _userDataService = userDataService;    
    }

    [HttpGet]
    [Route("/hello")]
    public ActionResult<string> ClownsWorld()
    {
        var userId = HttpContext?.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Missing sub claim");

        var name = _userDataService.GetUserDisplayName(userId);
        return Ok($"Hello, {name}! (UserID: {userId})");
    }
}