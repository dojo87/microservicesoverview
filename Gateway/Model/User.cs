using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserApi.Model
{
    public class User
    {
        public string Name { get; set;  }
        public string Password { get; set; }
        public string Claims { get; set; }
        public string Token { get; set; }

        public User CloneWithoutSensitiveData()
        {
            return new User()
            {
                Name = this.Name,
                Claims = this.Claims
            };
        }
    }
}
