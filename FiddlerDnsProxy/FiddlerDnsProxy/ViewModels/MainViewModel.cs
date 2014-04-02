using FiddlerDnsProxy.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FiddlerDnsProxy.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string relayDnsServer = "8.8.8.8";
        private readonly EventPubSub eventPubSub;
        public MainViewModel()
        {
            eventPubSub = new EventPubSub();
            var IpHelper = new IpAddressHelper();
            IpAddresses = new ObservableCollection<string>(IpHelper.GetIpAddressString());
            RedirectRecord = "";


            SetupDelegateCmds(relayDnsServer);

            Log = new ObservableCollection<string>();
            eventPubSub.Subscribe<FiddlerEvent>(x => AddLogEntryOnDispatcher(x.Log));
            eventPubSub.Subscribe<DnsEvent>(x => AddLogEntryOnDispatcher(x.Message));
        }

        private async Task AddLogEntryOnDispatcher(string log)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(()=>
            {
                Log.Add(log);
            }));
        }

        private void SetupDelegateCmds(string remoteDnsServerIp)
        {
            StartStopDnsServer = new DelegateCommand(() =>
            {
                if (DnsServerInstance != null)
                {
                    DnsServerInstance.Dispose();
                    DnsServerInstance = null;
                    FiddlerServerInstance.Dispose();
                    FiddlerServerInstance = null;
                }
                else
                {
                    DnsServerInstance = new InterceptingDnsServer(eventPubSub, new PortForwardingManager());
                    DnsServerInstance.Start(remoteDnsServerIp, SelectedIp, RedirectRecord);

                    FiddlerServerInstance = new FiddlerWrapper(eventPubSub);
                    FiddlerServerInstance.Start(80);
                }
                NotifyPropertyChanged("DnsServerInstance");
            });
        }

        public FiddlerWrapper FiddlerServerInstance { get; set; }
        public InterceptingDnsServer DnsServerInstance { get; set; }

        public ObservableCollection<string> IpAddresses { get; set; }
        public ObservableCollection<string> Log { get; set; }


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
