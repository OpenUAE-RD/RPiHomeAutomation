using System.Collections.Generic;
using Xamarin.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace AutoRPi
{
    public partial class MainPage : ContentPage
    {
        UdpClient udp;
        TcpClient tcpClient;
        TcpListener tcpListener;
        IPEndPoint udpEnd, tcpEnd;
        int udpPort = 20253, tcpPort = 20254;

        public MainPage()
        {
            InitializeComponent();

            udpEnd = new IPEndPoint(IPAddress.Broadcast, udpPort);
            tcpEnd = new IPEndPoint(IPAddress.Any, tcpPort);
            udp = new UdpClient(udpPort, AddressFamily.InterNetwork);

        }

        void ScanForRPi()
        {
            byte[] m = System.Text.Encoding.ASCII.GetBytes("rpi");
            udp.Send(m, m.Length, udpEnd);

            string data = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                data = System.Text.Encoding.ASCII.GetString(udp.Receive(ref udpEnd));
                if (data == "rpi")
                    continue;
                else
                    break;
            }

            int port = int.Parse(data);

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            tcpListener.Stop();

            DisplayAlert("Connected", tcpEnd.ToString(), "OK");
            tcpClient.GetStream().Write(m, 0, m.Length);
        }

        void Clicked(object sender, System.EventArgs e)
        {
            ScanForRPi();
        }
    }
}