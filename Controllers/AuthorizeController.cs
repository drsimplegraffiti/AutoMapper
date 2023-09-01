using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Techie.Modal;
using Techie.Repos;
using Techie.Repos.Models;

namespace Techie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizeController : ControllerBase
    {
        private readonly LearnDataContext _context;
        private readonly JwtSettings _jwtsettings;
        private readonly IMapper _mapper;
        public AuthorizeController(
            LearnDataContext context,
            IOptions<JwtSettings> options,
            IMapper mapper
            )
        {
            _context = context;
            _jwtsettings = options.Value;
            _mapper = mapper;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.Code == userCred.UserName && item.Password == userCred.Password);
            if (user == null)
                return NotFound("Invalid username or password");
            var token = GenerateToken(user);
            Dictionary<string, string> tokenResponse = new Dictionary<string, string>();
            tokenResponse.Add("token", token);
            return Ok(tokenResponse);
        }

        // register a user
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserModel data)
        {
            if(!ModelState.IsValid)
                return BadRequest("Invalid data" + ModelState);
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

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Code),
                    new Claim(ClaimTypes.Role, user.Role!)

                }),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}