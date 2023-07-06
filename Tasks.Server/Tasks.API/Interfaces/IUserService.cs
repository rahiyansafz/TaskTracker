using Tasks.API.Requests;
using Tasks.API.Responses;

namespace Tasks.API.Interfaces;

public interface IUserService
{
    Task<TokenResponse> LoginAsync(LoginRequest loginRequest);
    Task<SignupResponse> SignupAsync(SignupRequest signupRequest);
    Task<LogoutResponse> LogoutAsync(int userId);
    Task<UserResponse> GetInfoAsync(int userId);
}