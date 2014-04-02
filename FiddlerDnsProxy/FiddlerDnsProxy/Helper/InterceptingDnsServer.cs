using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using FiddlerDnsProxy.Helper;

namespace FiddlerDnsProxy
{
    public class InterceptingDnsServer : IDisposable
    {
        string _endDnsIp;
        string _serverIp;
        string _redirectIp;
        const int _httpsPort = 443;
        string _redirectRecord;
        DnsServer _server;
        PortForwardingManager _portMan;
        EventPubSub _eventPubSub;
        public InterceptingDnsServer(EventPubSub eventPubSub, PortForwardingManager portman = null)
        {
            _portMan = portman;
            _eventPubSub = eventPubSub;
        }

        public void Start(string endDnsIp, string serverIp, string redirectRecord)
        {
            _endDnsIp = endDnsIp;
            _serverIp = serverIp;
            _redirectRecord = redirectRecord;
            _server = new DnsServer(IPAddress.Any, 10, 10, ProcessQuery);
            _server.Start();
        }

        public void Dispose()
        {
            try
            {
                _server.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to stop dns server {0}", ex.ToString());
            }
        }

        private void LogMessage(IPAddress clientIp, string message)
        {
            var dnsEvent = new DnsEvent() { ClientIpAddress = clientIp.ToString(), Message = message };
            _eventPubSub.Publish<DnsEvent>(dnsEvent);
        }

        DnsMessageBase ProcessQuery(DnsMessageBase message, IPAddress clientAddress, ProtocolType protocol)
        {
            message.IsQuery = false;
            DnsMessage query = message as DnsMessage;
            var question = query.Questions.First();

            CheckIPAllowedAccess(clientAddress, question);

            DnsMessage answer = ResolveDnsQuery(question);

            if (IsRedicted(question))
            {
                LogMessage(clientAddress, string.Format("Resolved WITH redirection for {0}", question.Name));

                SetupReponse(query, question);

                if (_portMan != null)
                {
                    SetupForwarding(answer);
                }

                return query;
            }

            if ((query != null) && (query.Questions.Count == 1))
            {
                // send query to upstream server

                // if got an answer, copy it to the message sent to the client
                if (answer != null)
                {
                    foreach (DnsRecordBase record in (answer.AnswerRecords))
                    {
                        query.AnswerRecords.Add(record);
                    }
                    foreach (DnsRecordBase record in (answer.AdditionalRecords))
                    {
                        query.AnswerRecords.Add(record);
                    }

                    LogMessage(clientAddress, string.Format("Resolved WITHOUT redirection for {0}", question.Name));

                    query.ReturnCode = ReturnCode.NoError;
                    return query;
                }
            }

            // Not a valid query or upstream server did not answer correct
            LogMessage(clientAddress, string.Format("Failed to resolve {0}", question.Name));
            message.ReturnCode = ReturnCode.ServerFailure;
            return message;
        }

        private void SetupForwarding(DnsMessage answer)
        {
            if (answer == null)
            {
                return;
            }

            var response = answer.AnswerRecords.OfType<ARecord>().FirstOrDefault();

            if (response != null && response.Address.ToString() != _redirectIp)
            {
                Console.WriteLine("{0} : Found a record to proxy with https", response.Name);

                _redirectIp = response.Address.ToString();

                _portMan.StartForwarding(_httpsPort, _redirectIp);
            }
        }



        private DnsMessage ResolveDnsQuery(DnsQuestion question)
        {

            var ips = new List<IPAddress>() { IPAddress.Parse(_endDnsIp) };
            var client = new DnsClient(ips, 8000);
            DnsMessage answer = client.Resolve(question.Name, question.RecordType, question.RecordClass);

            if (answer == null)
            {
                LogMessage(null, string.Format("Failed to resolve DNS Name for {0}", question.Name));
            }

            return answer;
        }

        private bool IsRedicted(DnsQuestion question)
        {
            return (question.Name.Contains(_redirectRecord) || string.IsNullOrEmpty(_redirectRecord)) && question.RecordType == RecordType.A;
        }

        private void CheckIPAllowedAccess(IPAddress clientAddress, DnsQuestion question)
        {
            //impliment client ip checking - throw exception if not allowed. 
        }

        private void SetupReponse(DnsMessage query, DnsQuestion question)
        {
            var ip = IPAddress.Parse(_serverIp);
            ARecord record = new ARecord(question.Name, 100, ip);
            query.AnswerRecords.Add(record);
            query.ReturnCode = ReturnCode.NoError;
        }


    }
}

