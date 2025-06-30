using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace WebApi.Tests
{
    public class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; }

        public FakeHttpContextAccessor(ClaimsPrincipal user)
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            };
        }
    }
}