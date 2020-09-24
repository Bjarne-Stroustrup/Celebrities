using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Celebrities.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Celebrities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController: Controller
    {
        private readonly CelebritiesDbContext _celebritiesDbContext;
        private readonly SymmetricSecurityKey _symmetricSecurityKey;
        private readonly int _lifetime;

        public AccountController(CelebritiesDbContext celebritiesDbContext, IConfiguration configuration)
        {
            _celebritiesDbContext = celebritiesDbContext;

            var key = configuration.GetSection("Authentication:Secret").Value;
            _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

            _lifetime = int.Parse(configuration.GetSection("Authentication:Lifetime").Value);
        }

        [HttpPost("/token")]
        public IActionResult GetToken(string login, string password)
        {
            var identity = GetIdentity(login, password);
            if (identity == null)
            {
                return BadRequest(new { errorMessage = "Invalid login or password." });
            }

            var now = DateTime.UtcNow;

            var jwtToken = new JwtSecurityToken(
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(_lifetime)),
                signingCredentials: new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256));
            var encodedJwtToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var response = new
            {
                token = encodedJwtToken,
                lifetime = _lifetime
            };

            return Json(response);
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            var user = _celebritiesDbContext.Users.FirstOrDefault(u => u.Login == username && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }
    }
}