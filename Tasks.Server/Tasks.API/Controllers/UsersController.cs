using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Tasks.API.Interfaces;

using Tasks.API.Requests;

using Tasks.API.Responses;

namespace Tasks.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UsersController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (loginRequest is null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new TokenResponse
            {
                Error = "Missing login details",
                ErrorCode = "L01"
            });
        }

        var loginResponse = await _userService.LoginAsync(loginRequest);

        if (!loginResponse.Success)
        {
            return Unauthorized(new
            {
                loginResponse.ErrorCode,
                loginResponse.Error
            });
        }

        return Ok(loginResponse);
    }

    [HttpPost]
    [Route("refresh_token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        if (refreshTokenRequest is null || string.IsNullOrEmpty(refreshTokenRequest.RefreshToken) || refreshTokenRequest.UserId == 0)
        {
            return BadRequest(new TokenResponse
            {
                Error = "Missing refresh token details",
                ErrorCode = "R01"
            });
        }

        var validateRefreshTokenResponse = await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequest);

        if (!validateRefreshTokenResponse.Success)
            return BadRequest(validateRefreshTokenResponse);

        var tokenResponse = await _tokenService.GenerateTokensAsync(validateRefreshTokenResponse.UserId);

        return Ok(new TokenResponse { AccessToken = tokenResponse.Item1, RefreshToken = tokenResponse.Item2 });
    }

    [HttpPost]
    [Route("signup")]
    public async Task<IActionResult> Signup(SignupRequest signupRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors.Select(c => c.ErrorMessage)).ToList();
            if (errors.Any())
            {
                return BadRequest(new TokenResponse
                {
                    Error = $"{string.Join(",", errors)}",
                    ErrorCode = "S01"
                });
            }
        }

        var signupResponse = await _userService.SignupAsync(signupRequest);

        if (!signupResponse.Success)
            return UnprocessableEntity(signupResponse);

        return Ok(signupResponse.Email);
    }

    [Authorize]
    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        var logout = await _userService.LogoutAsync(UserID);

        if (!logout.Success)
            return UnprocessableEntity(logout);

        return Ok();
    }

    [Authorize]
    [HttpGet]
    [Route("info")]
    public async Task<IActionResult> Info()
    {
        var userResponse = await _userService.GetInfoAsync(UserID);

        if (!userResponse.Success)
            return UnprocessableEntity(userResponse);

        return Ok(userResponse);
    }
}