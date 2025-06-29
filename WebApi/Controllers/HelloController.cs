
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    private readonly IUserDataService _userDataService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HelloController(IUserDataService userDataService, IHttpContextAccessor httpContextAccessor)
    {
        _userDataService = userDataService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Route("/hello")]
    public ActionResult<string> Get()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Missing sub claim");

        var name = _userDataService.GetUserDisplayName(userId);
        return Ok($"Hello, {name}! (UserID: {userId})");
    }
}