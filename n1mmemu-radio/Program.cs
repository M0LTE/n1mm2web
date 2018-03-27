using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace n1mmemu_radio
{
    class Program
    {

        static void Main(string[] args)
        {
            while (true)
            {
                var ipep = new IPEndPoint(IPAddress.Parse("10.45.0.255"), 12060);

                using (var client = new UdpClient(AddressFamily.InterNetwork))
                {
                    byte[] buf = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8""?><RadioInfo>
    <StationName>CW-STATION</StationName>
    <RadioNr>2</RadioNr>
    <Freq>2120000</Freq>
    <TXFreq>2120000</TXFreq>
    <Mode>CW</Mode>
    <OpCall>PA1M</OpCall>
    <IsRunning>False</IsRunning>
    <FocusEntry>12170</FocusEntry>
    <Antenna>2</Antenna>
    <Rotors>tribander</Rotors>
    <FocusRadioNr>2</FocusRadioNr>
    <IsStereo>False</IsStereo>
    <ActiveRadioNr>2</ActiveRadioNr>
</RadioInfo>");

                    client.Send(buf, buf.Length, ipep);
                    Console.WriteLine("Sending " + buf.Length + " bytes");
                    Thread.Sleep(25000);
                }
            }
        }
    }
}