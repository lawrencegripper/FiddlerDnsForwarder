using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const string relayDnsServer = "8.8.4.4";
        public MainViewModel()
        {
            var IpHelper = new IpAddressHelper();
            IpAddresses = new ObservableCollection<string>(IpHelper.GetIpAddressString());
            var remoteDnsServerIp = relayDnsServer;
            RedirectRecord = "bbc.co.uk";
            var serverIp = SelectedIp;
            StartStopDnsServer = new DelegateCommand(() =>
            {
                if (DnsServerInstance != null)
                {
                    DnsServerInstance.Dispose();
                    DnsServerInstance = null;
                }
                else
                {
                    DnsServerInstance = new InterceptingDnsServer(remoteDnsServerIp, serverIp, RedirectRecord, new PortForwardingManager());
                }
                NotifyPropertyChanged("DnsServerInstance");
            });
        }

        public InterceptingDnsServer DnsServerInstance { get; set; }

        public ObservableCollection<string> IpAddresses { get; set; }


        private string _selectedIP;

        public string SelectedIp
        {
            get { return _selectedIP; }
            set { _selectedIP = value; NotifyPropertyChanged(); }
        }

        private string _redirectRecord;

        public string RedirectRecord
        {
            get { return _redirectRecord; }
            set { _redirectRecord = value; NotifyPropertyChanged(); }
        }


        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; NotifyPropertyChanged(); }
        }

        public ICommand StartStopDnsServer { get; set; }

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
