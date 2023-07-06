using Tasks.API.Entities;
using Tasks.API.Requests;
using Tasks.API.Responses;

namespace Tasks.API.Interfaces;

public interface ITokenService
{
    Task<Tuple<string, string>> GenerateTokensAsync(int userId);
    Task<ValidateRefreshTokenResponse> ValidateRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    Task<bool> RemoveRefreshTokenAsync(User user);
}