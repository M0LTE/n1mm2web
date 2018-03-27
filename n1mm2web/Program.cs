using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace n1mm2web
{
    class Program
    {
        static string ftpUser, ftpPassword, ftpServer, ftpFilePath;
        static int ftpPort, n1mmPort;

        static int Main(string[] args)
        {
            FetchConfig();
            if (!CheckConfig())
                return -1;

            Log($"Listing for N1MM on port {n1mmPort}, uploading to ftp://{ftpUser.Replace("@", "%40")}:{new String('*', ftpPassword.Length)}@{ftpServer}:{ftpPort}/{ftpFilePath}");

            udpThread.Start();
            webThread.Start();
            Thread.CurrentThread.Join();
            return 0;
        }

        private static bool CheckConfig()
        {
            bool prob = false;

            if (string.IsNullOrWhiteSpace(ftpUser))
            {
                prob = true;
                Log("ftpUser configuration setting is empty");
            }

            if (string.IsNullOrWhiteSpace(ftpPassword))
            {
                prob = true;
                Log("ftpPassword configuration setting is empty");
            }

            if (string.IsNullOrWhiteSpace(ftpServer))
            {
                prob = true;
                Log("ftpServer configuration setting is empty");
            }

            if (string.IsNullOrWhiteSpace(ftpFilePath))
            {
                prob = true;
                Log("ftpFilePath configuration setting is empty");
            }

            return !prob;
        }

        private static void FetchConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(path: "/etc/n1mm2web.conf", optional: true, reloadOnChange: true)
                .Build();

            ftpUser = config["ftpUser"];
            ftpPassword = config["ftpPassword"];
            ftpServer = config["ftpServer"];
            ftpFilePath = config["ftpFilePath"];
            if (!int.TryParse(config["ftpPort"], out ftpPort))
            {
                ftpPort = 21;
            }
            if (!int.TryParse(config["n1mmPort"], out n1mmPort))
            {
                n1mmPort = 12060;
            }
        }

        static Thread udpThread = new Thread(new ThreadStart(UdpListener)) { IsBackground = true };

        static Thread webThread = new Thread(new ThreadStart(WebSender)) { IsBackground = true };

        static string BuildHtml(Dictionary<int, N1mmRadioInfoWrapper> dict)
        {
            var sb = new MyStringBuilder();
            sb.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"5\" style=\"border-collapse: collapse; text-align:center\" id=\"AutoNumber1\" align=\"left\">");
            sb.AppendLine("  <tbody>");
            
            if (dict.Any(kv => kv.Value.Age < TimeSpan.FromMinutes(30)))
            {
                foreach (var item in dict.OrderBy(kv => kv.Key))
                {
                    sb.AppendLine("    <tr>");
                    if (item.Value.Age > TimeSpan.FromMinutes(2))
                    {
                        sb.AppendLine($"      <td colspan=\"2\">");
                        sb.AppendLine($"        Radio {item.Value.Info.RadioNr}: Currently OFF AIR");
                        sb.AppendLine($"      </td>");
                    }
                    else
                    {
                        sb.AppendLine($"      <td class=\"radiofreq\" nowrap=\"nowrap\">");
                        sb.AppendLine($"        <div align=\"left\">");
                        sb.AppendLine($"          Radio {item.Value.Info.RadioNr}: {item.Value.Info.TXFreq_dHz / 100000.0:0.000} {item.Value.Info.Mode}");
                        sb.AppendLine($"        </div>");
                        sb.AppendLine($"      </td>");
                        sb.AppendLine($"      <td class=\"radiofreq\" nowrap=\"nowrap\">");
                        sb.AppendLine($"        Operator Callsign: {item.Value.Info.OpCall}");
                        sb.AppendLine($"      </td>");
                    }
                    sb.AppendLine($"    </tr>");
                }
            }
            else
            {
                sb.AppendLine($"    <tr>");
                sb.AppendLine($"      <td colspan=\"2\">");
                sb.AppendLine("         Station is off the air or not sending updates");
                sb.AppendLine($"      </td>");
                sb.AppendLine($"    </tr>");
            }
            sb.AppendLine($"    <tr><td colspan=\"2\">");
            sb.AppendLine("      Last update: {dt}Z");
            sb.AppendLine($"    </td></tr>");
            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        static string lastHtml;
        static DateTime lastUploadUtc;

        static string Timestamp(string html)
        {
            return html.Replace("{dt}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        static void WebSender()
        {
            while (true)
            {
                try
                {
                    string html;
                    lock (state)
                    {
                        html = BuildHtml(state);
                    }

                    if (html != lastHtml)
                    {
                        string timestampedHtml = Timestamp(html);

                        bool success = TryUpload(timestampedHtml);

                        if (success)
                        {
                            lastHtml = html;
                            lastUploadUtc = DateTime.UtcNow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Uncaught exception in WebSender: {0}", ex);
                }
                finally
                {
                    Thread.Sleep(10000);
                }
            }
        }

        static bool TryUpload(string html)
        {
            Log("Uploading");

            try
            {
                string uri = $"ftp://{ftpServer}:{ftpPort}/{ftpFilePath}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                byte[] fileBytes = Encoding.UTF8.GetBytes(html);
                request.ContentLength = fileBytes.Length;
                var requestStream = request.GetRequestStream();
                requestStream.Write(fileBytes, 0, fileBytes.Length);
                requestStream.Close();
                var response = (FtpWebResponse)request.GetResponse();

                Log($"FTP response: {(response.StatusDescription ?? "").Replace("\r", "").Replace("\n", " ")}");
                return true;
            }
            catch (Exception ex)
            {
                Log("Exception uploading: {0}", ex);
                return false;
            }
        }

        static void UdpListener()
        { 
            while (true)
            {
                var listener = new UdpClient(new IPEndPoint(IPAddress.Any, 12060));

                while (true)
                {
                    IPEndPoint receivedFrom = new IPEndPoint(IPAddress.Any, 0);
                    byte[] msg = listener.Receive(ref receivedFrom);

                    try
                    {
                        ProcessDatagram(msg);
                    }
                    catch (Exception ex)
                    {
                        Log("Uncaught exception in UdpListener: {0}", ex);
                    }
                }
            }
        }

        static void Log(string format, params object[] args)
        {
            Console.Write("{0:yyyy-MM-dd HH:mm:ss}Z ", DateTime.UtcNow);
            Console.WriteLine(format, args);
        }

        static void ProcessDatagram(byte[] msg)
        {
            try
            {
                if (N1mmRadioInfo.TryParse(msg, out N1mmRadioInfo ri))
                {
                    ProcessRadioInfo(ri);
                }
            }
            finally
            {
                // could write out the datagram here
            }
        }

        /// <summary>
        /// Dictionary of RadioNr : latest state packet received for that radio
        /// </summary>
        static Dictionary<int, N1mmRadioInfoWrapper> state = new Dictionary<int, N1mmRadioInfoWrapper>();
        static object lockObj = new object();

        private static void ProcessRadioInfo(N1mmRadioInfo ri)
        {
            lock (lockObj)
            {
                if (!state.ContainsKey(ri.RadioNr))
                {
                    state.Add(ri.RadioNr, new N1mmRadioInfoWrapper());
                }

                state[ri.RadioNr].ReceivedUTC = DateTime.UtcNow;
                state[ri.RadioNr].Info = ri;
            }
        }
    }

    class N1mmRadioInfoWrapper
    {
        public TimeSpan Age { get { return DateTime.UtcNow - ReceivedUTC; } }
        public DateTime ReceivedUTC { get; set; }
        public N1mmRadioInfo Info { get; set; }
    }

    class MyStringBuilder
    {
        StringBuilder mySb = new StringBuilder();

        public void AppendLine(string line)
        {
            mySb.Append(line);
            mySb.Append("\n");
        }

        public override string ToString()
        {
            return mySb.ToString();
        }
    }
}