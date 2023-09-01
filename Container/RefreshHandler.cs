using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Techie.Repos;
using Techie.Repos.Models;
using Techie.Service;

namespace Techie.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearnDataContext _context;

        public RefreshHandler(LearnDataContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateRefreshToken(int id)
        {
            var randomNumber = new byte[32]; // a byte array of 32 bytes i.e 256 bits
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                var existingToken = _context.RefreshTokens.FirstOrDefault(item => item.Userid == id.ToString());
                if (existingToken != null)
                {
                    existingToken.Refreshtoken = refreshToken;
                    _context.RefreshTokens.Update(existingToken);
                }
                else
                {
                    var newToken = new RefreshToken
                    {
                        // use the actual user Id not the username
                        Userid = id.ToString(),
                        Refreshtoken = refreshToken,
                        Tokenid = Guid.NewGuid().ToString()
                    };
                    await _context.RefreshTokens.AddAsync(newToken);
                }
                await _context.SaveChangesAsync();
                return refreshToken;

            }
        }
    }
}