using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;

namespace WebApi
{
    public static class JWTOptionSelector
    {
        public static JwtBearerOptions GetJwtBearerOptions(JwtBearerOptions options,IWebHostEnvironment env)
        {
            if (env == null)
                throw new ArgumentNullException("[STARTUP] environment is null when deciding on JWT Bearer options");

            if (env.IsDev())
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    RequireExpirationTime = false,
                    SignatureValidator = (token, parameters) => new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token)
                };
            }
            throw new Exception("[STARTUP] no Environment defined !");            
        }
    }
}
