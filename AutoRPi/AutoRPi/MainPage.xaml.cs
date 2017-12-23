using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AutoRPi
{
    public partial class MainPage : ContentPage
    {
        UdpClient udp;
        TcpClient tcpClient;
        IPEndPoint udpEnd, tcpEnd;
        NetworkStream ns;
        IPAddress[] localIps;
        int udpPort = 20253, tcpPort = 20254;

        public MainPage()
        {
            InitializeComponent();

            udpEnd = new IPEndPoint(IPAddress.Broadcast, udpPort);
            tcpEnd = new IPEndPoint(IPAddress.Any, tcpPort);
            udp = new UdpClient(udpPort, AddressFamily.InterNetwork);
            localIps = Dns.GetHostAddresses(Dns.GetHostName());
        }

        bool IsOwnIp(IPAddress ip)
        {
            foreach (var i in localIps)
            {
                if (i.Equals(ip))
                    return true;
            }

            return false;
        }

        void ScanForRPi()
        {
            byte[] m = GetBytes("rpi");
            for (int tries = 0; tries < 3; tries++)
            {
                //Send broadcast
                udp.Send(m, m.Length, udpEnd);

                //Try to get a port response
                string data = string.Empty;
                bool gotReply = false;
                for (int i = 0; i < 5; i++)
                {
                    var udpAsync = udp.BeginReceive(null, null);
                    if (udpAsync.AsyncWaitHandle.WaitOne(200, false))
                    {
                        //Ignore own broadcast
                        data = GetString(udp.EndReceive(udpAsync, ref tcpEnd));
                        if (IsOwnIp(tcpEnd.Address))
                        {
                            continue;
                        }

                        gotReply = true;
                        break;
                    }
                }

                if (!gotReply)
                    continue;
                if (tcpClient != null && tcpClient.Connected)
                    tcpClient.Close();

                //Try to connect
                int port = int.Parse(data);
                tcpClient = new TcpClient();
                System.IAsyncResult tcpAsync = tcpClient.BeginConnect(tcpEnd.Address, port, null, null);
                if (!tcpAsync.AsyncWaitHandle.WaitOne(250, false))
                    continue;

                tcpClient.EndConnect(tcpAsync);
                ns = tcpClient.GetStream();
                ns.WriteTimeout = 2;
                DisplayAlert("Connected", tcpEnd.ToString(), "OK");
                return;
            }

            DisplayAlert($"Failed to connect", "Could not connect to 'rpi'", "OK");
        }

        void Send(string s)
        {
            if (tcpClient == null || !tcpClient.Connected)
                return;

            byte[] b = GetBytes(s);

            try
            {
                ns.Write(b, 0, b.Length);
            }

            catch (System.Exception)
            {
                UpdateConnectionStatus();
                return;
            }

            ns.Flush();
        }

        void UpdateConnectionStatus()
        {
            //TODO: Handle dropped connection
        }

        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        byte[] GetBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        #region Click events
        void ConnectBtn_Clicked(object sender, System.EventArgs e)
        {
            ScanForRPi();
        }

        void SendBtn_Clicked(object sender, System.EventArgs e)
        {
            Send(rpiNameEntry.Text);
        }

        void AddDevice_Clicked(object sender, System.EventArgs e)
        {
            var a = new AddDevicePg();
            a.Disappearing += AddDevicePgDisappearing;
            Navigation.PushAsync(a);
        }

        void AddDevicePgDisappearing(object sender, System.EventArgs e)
        {
            AddDevicePg adp = (AddDevicePg)sender;

            if (adp.WasSuccessful())
                AddDevice(adp.GetDeviceName(), adp.GetPin());
        }

        void About_Clicked(object sender, System.EventArgs e)
        {

        }
        #endregion

        void AddDevice(string name, int pin)
        {
            stackLayout.Children.Add(new DeviceContentView(name, pin, this));
        }

        public void SwitchToggled(int pin, bool on)
        {
            string cmd = pin + (on ? "1" : "0");
            if (cmd.Length < 3)
                cmd.Insert(0, "0");

            Send(cmd);
        }

        public void EditClicked(string name, int pin, DeviceContentView dcv)
        {
            Navigation.PushAsync(new AddDevicePg(name, pin, dcv));
        }

        public void DeleteClicked(DeviceContentView dcv)
        {
            stackLayout.Children.Remove(dcv);
        }
    }
}