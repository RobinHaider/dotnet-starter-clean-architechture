using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IUserService
    {
        Task<string> LoginRefreshToken(AppUser user, string ipAddress, bool rememberMe, int refreshTokenValidityInDays = 7);
        Task<bool> LogoutToken(AppUser user, string refreshToken, string ipAddress);
        Task<RefreshTokenResponse> RefreshToken(string token, string ipAddress, int refreshTokenValidityInDays = 7);
        Task RevokeToken(string token, string ipAddress);
    }

    public class UserService : IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task<string> LoginRefreshToken(AppUser user, string ipAddress, bool rememberMe, int refreshTokenValidityInDays = 7)
        {
            var refreshToken = await generateRefreshToken(ipAddress, rememberMe, refreshTokenValidityInDays);
            user.RefreshTokens.Add(refreshToken);

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // save changes to db
            //_context.Update(user);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }


        public async Task<RefreshTokenResponse> RefreshToken(string token, string ipAddress, int refreshTokenValidityInDays = 7)
        {
            var user = await getUserByRefreshToken(token);
            if (user == null) return null;
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null) return null;

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(user);
                _context.SaveChanges();
            }

            if (!refreshToken.IsActive)
                return null;

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = await rotateRefreshToken(refreshToken, ipAddress, refreshTokenValidityInDays);
            user.RefreshTokens.Add(newRefreshToken);

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            await _context.SaveChangesAsync();



            return new RefreshTokenResponse { RefreshToken = newRefreshToken, User = user };
        }

        public async Task<bool> LogoutToken(AppUser user, string refreshToken, string ipAddress)
        {
            var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);
            if (oldToken == null) return false;
            if (!oldToken.IsActive) return false;

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // revoke token and save
            revokeRefreshToken(oldToken, ipAddress, "user logged out");
            _context.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
            var user = await getUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            //if (!refreshToken.IsActive)
            //    throw new Exception("Invalid token");

            // revoke token and save
            revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        private async Task<RefreshToken> rotateRefreshToken(RefreshToken refreshToken, string ipAddress, int refreshTokenValidityInDays)
        {
            var newRefreshToken = await generateRefreshToken(ipAddress, refreshToken.RememberMe, refreshTokenValidityInDays);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private async Task<RefreshToken> generateRefreshToken(string ipAddress, bool rememberMe, int refreshTokenValidityInDays)
        {
            var refreshToken = new RefreshToken
            {
                Token = await getUniqueToken(),
                // token is valid for 7 days
                Expires = DateTime.UtcNow.AddDays(refreshTokenValidityInDays),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                RememberMe = rememberMe,
            };

            return refreshToken;

            async Task<string> getUniqueToken()
            {
                // token is a cryptographically strong random sequence of values
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                // ensure token is unique by checking against db
                var tokenIsUnique = !(await _context.Users.AnyAsync(u => u.RefreshTokens.Any(t => t.Token == token)));

                if (!tokenIsUnique)
                    return await getUniqueToken();

                return token;
            }
        }

        private async Task<AppUser> getUserByRefreshToken(string token)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .Include(r => r.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            return user;
        }

        private void removeOldRefreshTokens(AppUser user, int refreshTokenTTLInDays = 2)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(refreshTokenTTLInDays) <= DateTime.UtcNow);
        }
        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
        private void revokeDescendantRefreshTokens(RefreshToken refreshToken, AppUser user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                if (childToken.IsActive)
                    revokeRefreshToken(childToken, ipAddress, reason);
                else
                    revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }
    }

    public class RefreshTokenResponse
    {
        public RefreshToken RefreshToken { get; set; }
        public AppUser User { get; set; }
    }
}
