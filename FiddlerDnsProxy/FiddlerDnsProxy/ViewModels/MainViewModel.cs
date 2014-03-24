using FiddlerDnsProxy.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FiddlerDnsProxy.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            IpString = new IpAddressHelper().GetIpAddressString();
            var remoteDnsServerIp = "8.8.8.8";
            var serverIp = SelectedIp;
            StartStopDnsServer = new DelegateCommand(() =>
            {
                using (InterceptingDnsServer dnsServer = new InterceptingDnsServer(remoteDnsServerIp, serverIp, RedirectRecord, new PortForwardingManager()))
                {
                    ConfigureEventsAndStartFiddler(80);
                    Console.WriteLine("Server running");
                    Console.ReadLine();
                }
            });
        }

        private string _ipString;

        public string IpString
        {
            get { return _ipString; }
            set { _ipString = value; NotifyPropertyChanged(); }
        }

        private string _selectedIP;

        public string SelectedIp
        {
            get { return _selectedIP; }
            set { _selectedIP = value; }
        }
        

        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; NotifyPropertyChanged(); }
        }

        public ICommand StartStopDnsServer;


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
