using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Resource
{
    public static class ConfigureJwtAuthentication
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection serviceCollection, string key)
        {
            serviceCollection.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(jwt =>
           {
               jwt.RequireHttpsMetadata = true;
               jwt.SaveToken = true;
               jwt.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   IssuerSigningKey = GetSecurityKey(key),
                   ValidateAudience = false,
                   ValidateIssuerSigningKey = true

               };
           });

            return serviceCollection;
        }

        private static SecurityKey GetSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Convert.FromBase64String(key));
        }
    }
}
