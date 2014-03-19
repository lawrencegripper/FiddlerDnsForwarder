using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiddlerDnsProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetIpsForNetworkAdapters();

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
