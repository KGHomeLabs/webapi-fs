using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

// Custom Authorization Attribute that actually checks claims
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasClaimAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _claimType;
    private readonly string? _claimValue;

    public HasClaimAttribute(string claimType, string? claimValue = null)
    {
        _claimType = claimType;
        _claimValue = claimValue;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        // Check if user is authenticated
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        // Check if user has the required claim
        bool hasClaim;

        if (string.IsNullOrEmpty(_claimValue))
        {
            // Just check if claim type exists
            hasClaim = user.Claims.Any(c => c.Type == _claimType);
        }
        else
        {
            // Check for specific claim type and value
            hasClaim = user.HasClaim(_claimType, _claimValue);
        }

        if (!hasClaim)
        {
            context.Result = new UnauthorizedResult(); // 403
            return;
        }
    }
}