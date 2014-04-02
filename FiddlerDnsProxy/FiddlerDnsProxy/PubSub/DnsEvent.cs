using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    public class DnsEvent
    {
        public string ClientIpAddress { get; set; }
        public string Message { get; set; }
    }
}
