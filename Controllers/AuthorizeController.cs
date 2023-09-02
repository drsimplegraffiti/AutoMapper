using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Techie.Modal;
using Techie.Repos;
using Techie.Repos.Models;
using Techie.Service;

namespace Techie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizeController : ControllerBase
    {
        private readonly LearnDataContext _context;
        private readonly JwtSettings _jwtsettings;
        private readonly IMapper _mapper;
        private readonly IRefreshHandler _refreshHandler;
        public AuthorizeController(
            LearnDataContext context,
            IOptions<JwtSettings> options,
            IMapper mapper,
            IRefreshHandler refreshHandler
            )
        {
            _context = context;
            _jwtsettings = options.Value;
            _mapper = mapper;
            _refreshHandler = refreshHandler;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data" + ModelState);
            var user = await _context.Users.FirstOrDefaultAsync(item => item.Code == userCred.UserName);
            if (user == null)
                return NotFound("Invalid username or password");
            var password = BCrypt.Net.BCrypt.Verify(userCred.Password, user.Password);
          
            if (!password)
                return NotFound("Invalid username or password");
            var token = GenerateToken(user);
            // add refresh token to cookie
            Response.Cookies.Append("refreshToken", _refreshHandler.GenerateRefreshToken(user.Id).Result);
            return Ok(
                new
                {
                    token = token,
                    refreshToken = _refreshHandler.GenerateRefreshToken(user.Id).Result
                }
                );
        }

        // register a user
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserModel data)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data" + ModelState);
            // check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(item => item.Code == data.Code);
            if (existingUser != null)
                return BadRequest("User already exists");
            data.Password = HashPassword(data.Password);

            var user = _mapper.Map<User>(data); // map UserModel to User i.e convert data(UserModel) to user(User Entity)
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully");
        }

        // Delete a user
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == id);
            if (user == null)
                return NotFound("User not found");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted successfully");
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenResponse token)
        {
            var _refreshtoken = await _context.RefreshTokens.FirstOrDefaultAsync(item => item.Refreshtoken == token.RefreshToken);
            if (_refreshtoken != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(_jwtsettings.SecurityKey);
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token.Token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenkey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);

                var _token = validatedToken as JwtSecurityToken;
                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {

                    string userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value!; // the ! is to tell the compiler that the value is not null
                    var _existingdata = await _context.RefreshTokens.FirstOrDefaultAsync(item => item.Userid == userId && item.Refreshtoken == token.RefreshToken);
                    if (_existingdata != null)
                    {
                        var _newtoken = new JwtSecurityToken(
                             claims: principal.Claims.ToArray(),
                             expires: DateTime.UtcNow.AddSeconds(30),
                             signingCredentials: new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256Signature)
                         );
                        var _finaltoken = tokenHandler.WriteToken(_newtoken);
                        return Ok(new TokenResponse
                        {
                            Token = _finaltoken,
                            RefreshToken = _refreshHandler.GenerateRefreshToken(int.Parse(userId)).Result
                        });
                    }
                }
            }
            return BadRequest("Invalid token");
        }

        // bcrypt password hashing
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }



        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Code),
                    new Claim(ClaimTypes.Role, user.Role!)

                }),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var finalToken = tokenHandler.WriteToken(token);
            return new TokenResponse
            {
                Token = finalToken,
                RefreshToken = _refreshHandler.GenerateRefreshToken(user.Id).Result
            }.Token;

        }
    }
}