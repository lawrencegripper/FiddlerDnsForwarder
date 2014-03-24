using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    class IpAddressHelper
    {
        public IEnumerable<string> GetIpAddressString()
        {
            return GetIpsForNetworkAdapters().Select(x=>x.ToString());
        }

        private IEnumerable<IPAddress> GetIpsForNetworkAdapters()
        {

            var nics2 = from i in NetworkInterface.GetAllNetworkInterfaces()
                        where i.OperationalStatus == OperationalStatus.Up
                        select new { name = i.Name, ip = GetIpFromUnicastAddresses(i) };

            return nics2.Select(x => x.ip);
        }

        private IPAddress GetIpFromUnicastAddresses(NetworkInterface i)
        {
            return (from ip in i.GetIPProperties().UnicastAddresses
                    where ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    select ip.Address).SingleOrDefault();
        }
    }
}
