using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerDnsProxy
{
    class InterceptingDnsServer : IDisposable
    {
        readonly string _endDnsIp;
        readonly string _serverIp;
        string _redirectIp;
        const int _httpsPort = 443;
        readonly string _redirectRecord;
        readonly DnsServer _server;
        readonly PortForwardingManager _portMan;
        public InterceptingDnsServer(string endDnsIp, string serverIp, string redirectRecord, PortForwardingManager portman)
        {
            _endDnsIp = endDnsIp;
            _serverIp = serverIp;
            _redirectRecord = redirectRecord;
            _portMan = portman;
            _server = new DnsServer(IPAddress.Any, 10, 10, ProcessQuery);
        }

        public void Dispose()
        {
            _server.Stop();
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
                Console.WriteLine("Redirecting");
                SetupReponse(query, question);

                SetupForwarding(answer);

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

                    Console.WriteLine("{0} : Resolved ", question.Name);

                    query.ReturnCode = ReturnCode.NoError;
                    return query;
                }
            }

            // Not a valid query or upstream server did not answer correct
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
            var client = new DnsClient(ips, 1000);
            DnsMessage answer = client.Resolve(question.Name, question.RecordType, question.RecordClass);
            return answer;
        }

        private bool IsRedicted(DnsQuestion question)
        {
            return question.Name.Contains(_redirectRecord) && question.RecordType == RecordType.A;
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

