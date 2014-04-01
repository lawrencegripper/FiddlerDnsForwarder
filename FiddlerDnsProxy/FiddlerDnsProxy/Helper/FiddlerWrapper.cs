//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FiddlerDnsProxy.Helper
//{
//    public class FiddlerWrapper : IDisposable
//    {

//        public FiddlerWrapper(int port)
//        {
//            FiddlerApplication.Log.OnLogString += Log_OnLogString;
//            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
//            FiddlerApplication.Startup(port, false, false);
//        }

//        private static Proxy AttachProxyEndpoint(int port)
//        {
//            var altEndPoint = FiddlerApplication.CreateProxyEndpoint(port, true, "api-global.netflix.com");
//            if (altEndPoint == null)
//            {
//                Console.WriteLine("endpoint already in use on port {0}. Not binding", port);
//            }
//            else
//            {
//                //altEndPoint.Attach();
//                Console.WriteLine("Endpoint bound to port {0}", port);
//            }
//            return altEndPoint;
//        }

//        static void Log_OnLogString(object sender, LogEventArgs e)
//        {
//            Console.WriteLine("Fiddler Log message: {0}", e.LogString);
//        }

//        static void FiddlerApplication_BeforeRequest(Session oS)
//        {

//            Console.WriteLine("Fiddler Proxied request for url: {0}", oS.fullUrl);
//        }
//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
