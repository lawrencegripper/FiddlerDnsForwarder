using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    public class PortForwardingManager
    {
        readonly List<PortForwardingWrapper> currentForwarders = new List<PortForwardingWrapper>();
        public PortForwardingWrapper StartForwarding(int port, string remoteIp)
        {
            cancelForwarders();
            var remoteEndPoint = new Func<IPEndPoint>(() => GetRedirectEndPoint(port, remoteIp));
            var localEndPoint = new Func<IPEndPoint>(() => GetLocalEndPoint(port));
            var portFWrapper = new PortForwardingWrapper(localEndPoint, remoteEndPoint);
            currentForwarders.Add(portFWrapper);
            return portFWrapper;
        }

        private void cancelForwarders()
        {
            foreach (var f in currentForwarders)
            {
                f.Stop();
            }
        }

        private IPEndPoint GetRedirectEndPoint(int port, string remoteIp)
        {
            return new IPEndPoint(IPAddress.Parse(remoteIp), port);
        }

        private IPEndPoint GetLocalEndPoint(int port)
        {
            return new IPEndPoint(IPAddress.Any, port);
        }
    }
}
