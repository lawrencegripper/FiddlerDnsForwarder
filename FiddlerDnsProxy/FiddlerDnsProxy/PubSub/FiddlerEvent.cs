using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    public class FiddlerEvent
    {
        public string Log { get; set; }
        public Session Session { get; set; }
    }
}
