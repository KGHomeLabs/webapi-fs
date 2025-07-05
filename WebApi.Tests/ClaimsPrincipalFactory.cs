using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Tests
{
    public static class ClaimsPrincipalFactory
    {
        public static ClaimsPrincipal CreateUser(string userId, params (string type, string value)[] additionalClaims)
        {
            var claims = new List<Claim> { new Claim("sub", userId) };
            claims.AddRange(additionalClaims.Select(c => new Claim(c.type, c.value)));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        public static ClaimsPrincipal CreateAdmin(string userId) =>
            CreateUser(userId, ("role", "admin"), ("department", "IT"));

        public static ClaimsPrincipal CreateRegularUser(string userId) =>
            CreateUser(userId, ("role", "user"));
    }
}
