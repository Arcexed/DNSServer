using ARSoft.Tools.Net.Dns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using System.Net;

namespace DNSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (DnsServer server = new DnsServer(IPAddress.Any, 10, 10))
            {
                server.QueryReceived += OnQueryReceived;

                server.Start();

                Console.WriteLine("Press any key to stop server");
                Console.ReadLine();
            }
        }

        static async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
        {
            try
            {
                DnsMessage message = e.Query as DnsMessage;

                if (message == null)
                    return;

                DnsMessage response = message.CreateResponseInstance();

                if ((message.Questions.Count == 1))
                {
                    // send query to upstream server
                    DnsQuestion question = message.Questions[0];
                    DnsMessage upstreamResponse =
                        await DnsClient.Default.ResolveAsync(question.Name, question.RecordType, question.RecordClass);
                    string from = $"{e.RemoteEndpoint.Address.ToString()}";
                    var labels = question.Name.Labels;
                    var name = "";
                    for (int i = 0; i < labels.Length; i++)
                    {
                        name += labels[i];

                        if (i != labels.Length - 1)
                        {
                            name += ".";
                        }
                        
                    }

                    var sites=File.ReadAllLines("sites.txt");
                    if (!sites.Contains(name))
                    {
                        File.AppendAllText("sites.txt",$"{name}\n");
                    }
                    string questionDns = $"{from} {name} {question.RecordClass} {question.RecordType}";
                    //string responseDns = $"{upstreamResponse.} {question.RecordClass} {question.RecordType}";
                    Console.WriteLine(questionDns);
                    // if got an answer, copy it to the message sent to the client
                    if (upstreamResponse.AnswerRecords != null)
                    {
                        foreach (DnsRecordBase record in (upstreamResponse.AnswerRecords))
                        {
                            response.AnswerRecords.Add(record);
                        }

                        foreach (DnsRecordBase record in (upstreamResponse.AdditionalRecords))
                        {
                            response.AdditionalRecords.Add(record);
                        }

                        response.ReturnCode = ReturnCode.NoError;

                        // set the response
                        e.Response = response;
                    }
                    else
                    {
                        //  response.ReturnCode = ReturnCode.NotImplemented;
                        response.ReturnCode = ReturnCode.NxDomain;
                        e.Response = response;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
