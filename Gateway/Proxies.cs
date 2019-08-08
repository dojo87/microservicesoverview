using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway
{
    public class Proxies 
    {
        public string ProxyUrlPrefix { get; set; }
        public Proxy[] List { get; set; }
    }

    public class Proxy
    {
        public string Api { get; set; }
        public string Url { get; set; }

    }
}
