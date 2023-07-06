using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace Tasks.API.Controllers;

public class BaseController : ControllerBase
{
    protected int UserID => int.Parse(FindClaim(ClaimTypes.NameIdentifier));
    private string FindClaim(string claimName)
    {

        var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;

        var claim = claimsIdentity?.FindFirst(claimName);

        if (claim is null)
            return null!;

        return claim.Value;
    }
}