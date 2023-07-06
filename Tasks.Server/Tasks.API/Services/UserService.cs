using Microsoft.EntityFrameworkCore;

using Tasks.API.Entities;
using Tasks.API.Helpers;
using Tasks.API.Interfaces;
using Tasks.API.Requests;
using Tasks.API.Responses;

namespace Tasks.API.Services;

public class UserService : IUserService
{
    private readonly TasksDbContext _tasksDbContext;
    private readonly ITokenService _tokenService;

    public UserService(TasksDbContext tasksDbContext, ITokenService tokenService)
    {
        _tasksDbContext = tasksDbContext;
        _tokenService = tokenService;
    }

    public async Task<UserResponse> GetInfoAsync(int userId)
    {
        var user = await _tasksDbContext.Users.FindAsync(userId);

        if (user is null)
        {
            return new UserResponse
            {
                Success = false,
                Error = "No user found",
                ErrorCode = "I001"
            };
        }

        return new UserResponse
        {
            Success = true,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreationDate = user.Ts
        };
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest loginRequest)
    {
        var user = _tasksDbContext.Users.SingleOrDefault(user => user.Active && user.Email == loginRequest.Email);

        if (user is null)
        {
            return new TokenResponse
            {
                Success = false,
                Error = "Email not found",
                ErrorCode = "L02"
            };
        }
        var passwordHash = PasswordHelper.HashUsingPbkdf2(loginRequest.Password, Convert.FromBase64String(user.PasswordSalt));

        if (user.Password != passwordHash)
        {
            return new TokenResponse
            {
                Success = false,
                Error = "Invalid Password",
                ErrorCode = "L03"
            };
        }

        var token = await System.Threading.Tasks.Task.Run(() => _tokenService.GenerateTokensAsync(user.Id));

        return new TokenResponse
        {
            Success = true,
            AccessToken = token.Item1,
            RefreshToken = token.Item2,
            UserId = user.Id,
            FirstName = user.FirstName
        };
    }

    public async Task<LogoutResponse> LogoutAsync(int userId)
    {
        var refreshToken = await _tasksDbContext.RefreshTokens.FirstOrDefaultAsync(o => o.UserId == userId);

        if (refreshToken is null)
            return new LogoutResponse { Success = true };

        _tasksDbContext.RefreshTokens.Remove(refreshToken);

        var saveResponse = await _tasksDbContext.SaveChangesAsync();

        if (saveResponse >= 0)
            return new LogoutResponse { Success = true };

        return new LogoutResponse { Success = false, Error = "Unable to logout user", ErrorCode = "L04" };
    }

    public async Task<SignupResponse> SignupAsync(SignupRequest signupRequest)
    {
        var existingUser = await _tasksDbContext.Users.SingleOrDefaultAsync(user => user.Email == signupRequest.Email);

        if (existingUser is not null)
        {
            return new SignupResponse
            {
                Success = false,
                Error = "User already exists with the same email",
                ErrorCode = "S02"
            };
        }

        if (signupRequest.Password != signupRequest.ConfirmPassword)
        {
            return new SignupResponse
            {
                Success = false,
                Error = "Password and confirm password do not match",
                ErrorCode = "S03"
            };
        }

        if (signupRequest.Password.Length <= 7) // This can be more complicated than only length, you can check on alphanumeric and or special characters
        {
            return new SignupResponse
            {
                Success = false,
                Error = "Password is weak",
                ErrorCode = "S04"
            };
        }

        var salt = PasswordHelper.GetSecureSalt();
        var passwordHash = PasswordHelper.HashUsingPbkdf2(signupRequest.Password, salt);

        var user = new User
        {
            Email = signupRequest.Email,
            Password = passwordHash,
            PasswordSalt = Convert.ToBase64String(salt),
            FirstName = signupRequest.FirstName,
            LastName = signupRequest.LastName,
            Ts = signupRequest.Ts,
            Active = true // You can save is false and send confirmation email to the user, then once the user confirms the email you can make it true
        };

        await _tasksDbContext.Users.AddAsync(user);

        var saveResponse = await _tasksDbContext.SaveChangesAsync();

        if (saveResponse >= 0)
            return new SignupResponse { Success = true, Email = user.Email };

        return new SignupResponse
        {
            Success = false,
            Error = "Unable to save the user",
            ErrorCode = "S05"
        };

    }
}
