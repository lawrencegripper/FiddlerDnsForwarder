using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//http://blog.brunogarcia.com/2012/10/simple-tcp-forwarder-in-c.html
namespace FiddlerDnsProxy
{
    public class TcpForwarderSlim
    {
        public readonly Socket MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public void Start(Func<IPEndPoint> localFunc, Func<IPEndPoint> remoteFunc)
        {
                var remote = remoteFunc.Invoke();
                var local = localFunc.Invoke();
            try
            {
                MainSocket.Bind(local);
                MainSocket.Listen(10);
                while (true)
                {
                    Console.WriteLine("{0} : Starting forwarding with new tunnel", remote.Address.ToString());
                    var source = MainSocket.Accept();
                    var destination = new TcpForwarderSlim();
                    var state = new State(source, destination.MainSocket);
                    destination.Connect(remote, source);
                    source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                    Console.WriteLine("{0} : Request Forwarded", remote.Address.ToString());
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("{0} :  Closing tunnel", remote.Address.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(MainSocket, destination);
            MainSocket.Connect(remoteEndpoint);
            MainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }
    }

}
