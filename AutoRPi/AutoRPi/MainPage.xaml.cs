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
        int udpPort = 20253;

        string connectedRPi = string.Empty;

        public MainPage()
        {
            InitializeComponent();

            udpEnd = new IPEndPoint(IPAddress.Broadcast, udpPort);
            tcpEnd = new IPEndPoint(IPAddress.Any, 0);
            localIps = Dns.GetHostAddresses(Dns.GetHostName());

            rpiPicker.SelectedItem = null;
            rpiPicker.Items.Add("y tho");
            rpiPicker.Items.Add("rpi");
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

        void ConnectToRPi()
        {
            //HACK: For some reason if this is not done udp will not receive after first connection
            if (udp != null)
                udp.Close();
            udp = new UdpClient(udpPort, AddressFamily.InterNetwork);

            string rpiName = rpiPicker.SelectedItem.ToString();
            byte[] m = GetBytes(rpiName);
            for (int tries = 0; tries < 3; tries++)
            {
                //Send broadcast
                try
                {
                    udp.Send(m, m.Length, udpEnd);
                }

                catch (System.Exception)
                {
                    break;
                }

                //Try to get a port response
                string data = string.Empty;
                bool gotReply = false;
                for (int i = 0; i < 7; i++)
                {
                    var udpAsync = udp.BeginReceive(null, null);
                    if (udpAsync.AsyncWaitHandle.WaitOne(250, false))
                    {
                        //Ignore own broadcast by checking IP of packet
                        data = GetString(udp.EndReceive(udpAsync, ref tcpEnd));
                        if (IsOwnIp(tcpEnd.Address))
                            continue;

                        gotReply = true;
                        break;
                    }
                }

                if (!gotReply)
                    continue;

                //Try to connect
                tcpEnd.Port = int.Parse(data);
                tcpClient = new TcpClient();
                System.IAsyncResult tcpAsync = tcpClient.BeginConnect(tcpEnd.Address, tcpEnd.Port, null, null);
                if (!tcpAsync.AsyncWaitHandle.WaitOne(250, false))
                    continue;

                tcpClient.EndConnect(tcpAsync);

                ns = tcpClient.GetStream();
                ns.WriteTimeout = 2;

                connectedRPi = rpiName;
                UpdateConnectionStatus();
                DisplayAlert("Connected", tcpEnd.ToString(), "OK");
                return;
            }

            connectedRPi = string.Empty;
            UpdateConnectionStatus();
            DisplayAlert($"Failed to connect", $"Could not connect to '{rpiName}'", "OK");
        }

        void Send(RPiCmds cmd)
        {
            Send(((int)cmd).ToString());
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
                connectedRPi = string.Empty;
                UpdateConnectionStatus();
                return;
            }

            ns.Flush();
        }

        void UpdateConnectionStatus()
        {
            if (connectedRPi == string.Empty)
            {
                connectionLabel.Text = "Not Connected";
                connectionLabel.TextColor = Color.Red;
            }

            else
            {
                connectionLabel.Text = $"Connected to '{connectedRPi}'";
                connectionLabel.TextColor = Color.Green;
            }
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
            //Ignore if trying to connect current rpi
            if (rpiPicker.SelectedItem == null || connectedRPi == rpiPicker.SelectedItem.ToString())
                return;

            if (tcpClient != null && tcpClient.Connected)
            {
                Send(RPiCmds.ClosePort);
                tcpClient.Close();
            }

            ConnectToRPi();
        }

        void AddDeviceClicked(object sender, System.EventArgs e)
        {
            var a = new AddDevicePg();
            a.Disappearing += AddDevicePgDisappearing;
            Navigation.PushAsync(a, true);
        }

        void AddDevicePgDisappearing(object sender, System.EventArgs e)
        {
            AddDevicePg adp = (AddDevicePg)sender;

            if (adp.WasSuccessful())
                AddDevice(adp.GetDeviceName(), adp.GetPin());
        }

        void AddRPiClicked(object sender, System.EventArgs e)
        {
            AddRPiPg arp = new AddRPiPg();
            arp.rpiAddedCallback = AddRPiName;
            Navigation.PushAsync(arp, true);
        }

        void AddRPiName(string name)
        {
            rpiPicker.Items.Add(name);
        }

        void AboutClicked(object sender, System.EventArgs e)
        {

        }

        public void SwitchToggled(int pin, bool on)
        {
            string cmd = pin + (on ? "1" : "0");
            if (cmd.Length < 3)
                cmd.Insert(0, "0");

            Send(cmd);
        }

        public void EditBtnClicked(string name, int pin, DeviceContentView dcv)
        {
            Navigation.PushAsync(new AddDevicePg(name, pin, dcv), true);
        }

        public void DeleteBtnClicked(DeviceContentView dcv)
        {
            stackLayout.Children.Remove(dcv);
        }
        #endregion

        void AddDevice(string name, int pin)
        {
            stackLayout.Children.Add(new DeviceContentView(name, pin, this));
        }
    }
}