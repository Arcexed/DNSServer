using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace DNSFlooder
{
    class Program
    {
        static void Main(string[] args)
        {
            DnsClient dnsClient = new DnsClient(IPAddress.Parse("127.0.0.1"), 100);

            while(true){
            DnsMessage dnsMessage = DnsClient.Default.Resolve(DomainName.Parse("www.example.com"), RecordType.A);
            if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
            {
                throw new Exception("DNS request failed");
            }
            else
            {
                foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                {
                    ARecord aRecord = dnsRecord as ARecord;
                    if (aRecord != null)
                    {
                        Console.WriteLine(aRecord.Address.ToString());
                    }
                }
            }
            }
        }
    }
}
