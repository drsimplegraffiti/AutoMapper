using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Techie.Repos;

namespace Techie.Helper
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly LearnDataContext _context;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, LearnDataContext context) : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("No header found");
            }

            var headerValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

            if (headerValue == null)
            {
                return AuthenticateResult.Fail("Invalid header");
            }

            if (!string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid scheme");
            }

            var parameterBytes = Convert.FromBase64String(headerValue.Parameter);
            var credentials = Encoding.UTF8.GetString(parameterBytes);
            var credentialParts = credentials.Split(':', 2);
            if (credentialParts.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid credentials format");
            }

            var username = credentialParts[0];
            var password = credentialParts[1];

            var user = await _context.Users.FirstOrDefaultAsync(item => item.Code == username && item.Password == password);

            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid username or password");
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, user.Code),
                // Add other claims as needed
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
