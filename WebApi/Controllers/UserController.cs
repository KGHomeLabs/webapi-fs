using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Database.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserDataService _userDataService;

        public UserController(ILogger<UserController> logger, IUserDataService userDataService)
        {
            _logger = logger;
            _userDataService = userDataService;
        }

        [HttpGet("me")]
        [HasClaim("sub")]  
        public ActionResult<UserDBO> GetCurrentUser()
        {
            _logger.LogInformation("GetCurrentUser endpoint called");

            var user = HttpContext.Items["UserDBO"] as UserDBO;
            if (user == null)
            {
                return StatusCode(500, "User data not found in context");
            }

            return Ok(user);
        }


        [HttpPost]
        [HasClaim("sub")]
        public async Task<ActionResult> CreateUser([FromBody] UserDBO user)
        {
            _logger.LogInformation($"CreateUser endpoint called for UserId: {user.UserId}");
            var existingUser = await _userDataService.GetUserById(user.UserId);
            if (existingUser != null)
            {
                return Conflict($"User with UserId {user.UserId} already exists");
            }
            await _userDataService.CreateUser(user);
            return CreatedAtAction(nameof(GetCurrentUser), new { userId = user.UserId }, user);
        }

        [HttpPut("{userId}")]
        [HasClaim("sub")]
        public async Task<ActionResult> UpdateUser(string userId, [FromBody] UserDBO user)
        {
            _logger.LogInformation($"UpdateUser endpoint called for UserId: {userId}");
            var currentUser = HttpContext.Items["UserDBO"] as UserDBO;
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return Forbid("Only admins can update users");
            }
            var existingUser = await _userDataService.GetUserById(userId);
            if (existingUser == null)
            {
                return NotFound($"User with UserId {userId} not found");
            }
            await _userDataService.UpdateUser(userId, new UserDBO
            {
                UserId = userId,
                IsAdmin = user.IsAdmin,
                IsRoot = existingUser.IsRoot, // Preserve IsRoot
                UserName = user.UserName,
                IsLockedOut = user.IsLockedOut
            });
            return NoContent();
        }

        [HttpDelete("{userId}")]
        [HasClaim("sub")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            _logger.LogInformation($"DeleteUser endpoint called for UserId: {userId}");
            var currentUser = HttpContext.Items["UserDBO"] as UserDBO;
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return Forbid("Only admins can delete users");
            }
            var existingUser = await _userDataService.GetUserById(userId);
            if (existingUser == null)
            {
                return NotFound($"User with UserId {userId} not found");
            }
            await _userDataService.DeleteUser(userId);
            return NoContent();
        }
    }
}