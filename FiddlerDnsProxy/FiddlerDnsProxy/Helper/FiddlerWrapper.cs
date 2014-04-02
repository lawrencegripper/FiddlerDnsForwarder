using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy.Helper
{
    public class FiddlerWrapper : IDisposable
    {

        public FiddlerWrapper(EventPubSub pubSub)
        {
            FiddlerApplication.Log.OnLogString += (s, o) => 
            {
                pubSub.Publish<FiddlerEvent>(new FiddlerEvent() { Log = o.LogString });
            };
            FiddlerApplication.BeforeRequest += (session) =>
            {
                pubSub.Publish<FiddlerEvent>(new FiddlerEvent() { Log = session.fullUrl, Session = session });
            };
        }

        public void Start(int port)
        {
            //try
            //{
                FiddlerApplication.Startup(port, false, false);            
            //}
        }

        public void Dispose()
        {
            FiddlerApplication.Shutdown();
        }
    }
}
