using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserApi.Model;

namespace Gateway.Service
{
    public interface IAuthService
    {
        User Authenticate(HttpContext context);
    }
}
