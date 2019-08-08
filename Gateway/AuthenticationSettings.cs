using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway
{
    public class AuthenticationSettings
    {
        public string JWTKey { get; set; }
        public long JWTExpirationMiliseconds { get; set; }
    }
}
