using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace WebApi.Tests.Extensions
{
    public static class HttpClientExtensions
    {
        public static void SetFakeJwtToken(this HttpClient client, params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create a simple token descriptor - since your service disables validation, 
            // we don't need real signing
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("fake-key-for-testing-only-needs-to-be-long-enough")),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);
        }
    }
}