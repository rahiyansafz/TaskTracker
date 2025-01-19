using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace Tasks.API.Controllers;

public class BaseController : ControllerBase
{
    protected int UserID {
        get
        {
            var claimValue = FindClaim(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimValue))
            {
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            return int.Parse(claimValue);
        }
    }
    
    private string? FindClaim(string claimName)
    {

        var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;

        var claim = claimsIdentity?.FindFirst(claimName);

        return claim?.Value;
    }
}