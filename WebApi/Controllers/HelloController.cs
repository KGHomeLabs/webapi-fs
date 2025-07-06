
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
[Authorize]
public class HelloController : ControllerBase
{
    public HelloController()
    {

    }

    [HttpGet]
    [Route("/hello")]
    [HasClaim("sub")]  
    [HasClaim("userFart")]  //TODO:  this could also be replaced by a policy... I will try after fixing Clerk in the frontend
    public ActionResult<string> ClownsWorld()
    {
        var userId = HttpContext?.User.FindFirst("sub")?.Value;
        var userFart = HttpContext?.User.FindFirst("userFart")?.Value;
        

        return Ok($"Hello, {userFart}! (UserID: {userId})");
    }
}
