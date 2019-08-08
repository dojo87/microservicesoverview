using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserApi.Model;

namespace Gateway.Service
{
    public class DefaultAuthService : IAuthService
    {
        private IOptions<AuthenticationSettings> settings;

        public DefaultAuthService(IOptions<AuthenticationSettings> settings)
        {
            this.settings = settings;
        }

        public User Authenticate(HttpContext context)
        {

            User result = FindUser(context.User.Identity.Name);
            if (result != null)
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityTokenDescriptor tokenDetails = new SecurityTokenDescriptor()
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(
                    new Claim[]{
                        new Claim(ClaimTypes.Name, result.Name),
                        new Claim(ClaimTypes.Role, result.Claims)
                    }),
                    Expires = DateTime.UtcNow.AddMilliseconds(this.settings.Value.JWTExpirationMiliseconds),
                    SigningCredentials = GetSecurityCredentials()
                };
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                result.Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDetails));
            }

            return result;

        }

        protected User FindUser(string user)
        {
            if (user == null) return null;
            return USERS_MOCK.FirstOrDefault(u => user.EndsWith(u.Name))?.CloneWithoutSensitiveData();
        }

        protected SigningCredentials GetSecurityCredentials()
        {
            var jwtKey = Convert.FromBase64String(this.settings.Value.JWTKey);
            return new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature);
        }
        

        private static List<User> USERS_MOCK = new List<User>()
        {
            new User()
            {
                Name = "a305477",
                Claims = "some additional info like roles and such"
            }
        };
    }
}
