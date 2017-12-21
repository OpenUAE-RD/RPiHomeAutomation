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
            tcpListener = new TcpListener(IPAddress.Any, tcpPort);

        }

        void ScanForRPi()
        {
            byte[] m = System.Text.Encoding.ASCII.GetBytes("rpi");
            udp.Send(m, m.Length, udpEnd);
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            tcpListener.Stop();

            b.Text = tcpClient.Connected.ToString();
            DisplayAlert("Connection", tcpEnd.ToString(), "OK");
        }

        void Clicked(object sender, System.EventArgs e)
        {
            ScanForRPi();
        }
    }
}