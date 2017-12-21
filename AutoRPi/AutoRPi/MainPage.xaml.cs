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
        NetworkStream ns;
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
            byte[] m = GetBytes("rpi");
            udp.Send(m, m.Length, udpEnd);

            string data = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                data = GetString(udp.Receive(ref udpEnd));
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
            ns = tcpClient.GetStream();
        }

        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        byte[] GetBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        void bBtn_Clicked(object sender, System.EventArgs e)
        {
            ScanForRPi();
        }

        void SendBtn_Clicked(object sender, System.EventArgs e)
        {
            Send(textBox.Text);
        }

        void Send(string s)
        {
            byte[] b = GetBytes(s);
            ns.Write(b, 0, b.Length);
            ns.Flush();
        }
    }
}