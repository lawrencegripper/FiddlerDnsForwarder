using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    public class PortForwardingWrapper : IDisposable
    {
        private CancellationTokenSource cancellationToken;
        private TcpForwarderSlim forwarder;
        public PortForwardingWrapper(Func<IPEndPoint> localFunc, Func<IPEndPoint> remoteFunc)
        {
            cancellationToken = new CancellationTokenSource();
            forwarder = new TcpForwarderSlim();
            var portForwardingTask = Task.Factory.StartNew((x) =>
            {
                forwarder.Start(localFunc, remoteFunc);
            }, cancellationToken); 
        }

        public void Stop()
        {
            forwarder.MainSocket.Close();
            cancellationToken.Cancel();
        }
        public void Dispose()
        {
            Stop();
        }
    }
}
