using Microsoft.EntityFrameworkCore;
using Tasks.API.Data;
using Tasks.API.Entities;
using Tasks.API.Helpers;
using Tasks.API.Interfaces;
using Tasks.API.Requests;
using Tasks.API.Responses;

namespace Tasks.API.Services;

public class TokenService(TasksDbContext tasksDbContext) : ITokenService
{
    public async Task<Tuple<string, string>> GenerateTokensAsync(int userId)
    {
        var accessToken = await TokenHelper.GenerateAccessToken(userId);
        var refreshToken = await TokenHelper.GenerateRefreshToken();

        var userRecord = await tasksDbContext.Users.Include(o => o.RefreshTokens).FirstOrDefaultAsync(e => e.Id == userId);

        if (userRecord is null)
            return null!;

        var salt = PasswordHelper.GetSecureSalt();

        var refreshTokenHashed = PasswordHelper.HashUsingPbkdf2(refreshToken, salt);

        if (userRecord.RefreshTokens.Count != 0)
        {
            await RemoveRefreshTokenAsync(userRecord);
        }
        userRecord.RefreshTokens?.Add(new RefreshToken
        {
            ExpiryDate = DateTime.Now.AddDays(14),
            Ts = DateTime.Now,
            UserId = userId,
            TokenHash = refreshTokenHashed,
            TokenSalt = Convert.ToBase64String(salt)

        });

        await tasksDbContext.SaveChangesAsync();

        var token = new Tuple<string, string>(accessToken, refreshToken);

        return token;
    }

    public async Task<bool> RemoveRefreshTokenAsync(User user)
    {
        var userRecord = await tasksDbContext.Users.Include(o => o.RefreshTokens).FirstOrDefaultAsync(e => e.Id == user.Id);

        if (userRecord is null)
            return false;

        if (userRecord.RefreshTokens.Count == 0) return false;
        var currentRefreshToken = userRecord.RefreshTokens.First();
        tasksDbContext.RefreshTokens.Remove(currentRefreshToken);

        return false;
    }

    public async Task<ValidateRefreshTokenResponse> ValidateRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var refreshToken = await tasksDbContext.RefreshTokens.FirstOrDefaultAsync(o => o.UserId == refreshTokenRequest.UserId);

        var response = new ValidateRefreshTokenResponse();
        if (refreshToken is null)
        {
            response.Success = false;
            response.Error = "Invalid session or user is already logged out";
            response.ErrorCode = "invalid_grant";
            return response;
        }

        var refreshTokenToValidateHash = PasswordHelper.HashUsingPbkdf2(refreshTokenRequest.RefreshToken, Convert.FromBase64String(refreshToken.TokenSalt));

        if (refreshToken.TokenHash != refreshTokenToValidateHash)
        {
            response.Success = false;
            response.Error = "Invalid refresh token";
            response.ErrorCode = "invalid_grant";
            return response;
        }

        if (refreshToken.ExpiryDate < DateTime.Now)
        {
            response.Success = false;
            response.Error = "Refresh token has expired";
            response.ErrorCode = "invalid_grant";
            return response;
        }

        response.Success = true;
        response.UserId = refreshToken.UserId;

        return response;
    }
}
