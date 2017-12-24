using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace AutoRPi
{
    public partial class MainPage : ContentPage
    {
        //TODO: Cleanup the way commands are handled
        UdpClient udp;
        TcpClient tcpClient;
        IPEndPoint udpEnd, tcpEnd;
        NetworkStream ns;
        IPAddress[] localIps;

        const int udpPort = 20253;
        readonly Color connectedColor = Color.Green, disconnectedColor = Color.Red, connectingColor = Color.Blue;
        readonly string saveDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        const string ext = ".rha";
        const int deviceContentOffset = 2;
        string connectedRPi = string.Empty;

        public MainPage()
        {
            InitializeComponent();

            udpEnd = new IPEndPoint(IPAddress.Broadcast, udpPort);
            tcpEnd = new IPEndPoint(IPAddress.Any, 0);
            localIps = Dns.GetHostAddresses(Dns.GetHostName());

            rpiPicker.SelectedItem = null;
            PopulateRPiPicker();
            //rpiPicker.Items.Add("y tho");
            //rpiPicker.Items.Add("rpi");
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
                SetConnectionLabel($"Connected to '{connectedRPi}'", connectedColor);
                DisplayAlert("Connected", tcpEnd.ToString(), "OK");
                return;
            }

            connectedRPi = string.Empty;
            SetConnectionLabel("Not Connected", disconnectedColor);
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
                SetConnectionLabel("Not Connected", disconnectedColor);
                return;
            }

            ns.Flush();
        }

        void SetConnectionLabel(string msg, Color c)
        {
            connectionLabel.Text = msg;
            connectionLabel.TextColor = c;
        }

        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        byte[] GetBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        #region Toolbar events
        void AddRPiClicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new AddRPiPg(rpiPicker.Items) { rpiAddedCallback = AddRPiCallback }, true);
        }

        void AddDeviceClicked(object sender, System.EventArgs e)
        {
            if (rpiPicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Please select a RPi to add to.", "OK");
                return;
            }

            var a = new AddDevicePg();
            a.Disappearing += AddDevicePgDisappearing;
            Navigation.PushAsync(a, true);
        }

        void ChangeRPiNameClicked(object sender, System.EventArgs e)
        {
            if (rpiPicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Please select a RPi first.", "OK");
                return;
            }

            Navigation.PushAsync(new AddRPiPg(rpiPicker.Items, rpiPicker.SelectedItem.ToString()) { rpiAddedCallback = AddRPiCallback }, true);
        }

        async void DeleteRPiClicked(object sender, System.EventArgs e)
        {
            if (rpiPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a RPi to remove.", "OK");
                return;
            }

            //Delete save file
            if ((await DisplayAlert("Are you sure?", "This action is NOT reversable. Continue?", "Yes", "No")))
            {
                string path = Path.Combine(saveDir, rpiPicker.SelectedItem.ToString()) + ext;
                if (File.Exists(path))
                    File.Delete(path);

                int i = rpiPicker.SelectedIndex;
                rpiPicker.SelectedItem = null;
                rpiPicker.Items.RemoveAt(i);

                Send(RPiCmds.ClosePort);
                if (tcpClient != null)
                    tcpClient.Close();
                SetConnectionLabel("Not Connected", Color.Red);
            }
        }

        void AboutClicked(object sender, System.EventArgs e)
        {
            DisplayAlert("About", "Raspberry Pi Home Automation\n\nBy: OpenUAE R&D Group\n\nVersion: 1.0", "Close");
        }
        #endregion

        #region Click events
        void ConnectBtnClicked(object sender, System.EventArgs e)
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

        public void SwitchToggled(int pin, bool on)
        {
            string cmd = pin + (on ? "1" : "0");
            if (cmd.Length < 3)
                cmd.Insert(0, "0");

            Send(cmd);
        }

        public void EditBtnClicked(string name, int pin, DeviceContentView dcv)
        {
            var a = new AddDevicePg(name, pin, dcv);
            a.Disappearing += DeviceContentFinishedEditing;
            Navigation.PushAsync(a, true);
        }

        public void DeleteBtnClicked(DeviceContentView dcv)
        {
            stackLayout.Children.Remove(dcv);
            Save();
        }
        #endregion

        /// <summary>
        /// Handles adding a RPi or changing its name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="update"></param>
        void AddRPiCallback(string name, bool update)
        {
            string newFile = Path.Combine(saveDir, name) + ext;
            File.Create(newFile).Close();

            //If we are changing the name then copy the saved data over and delete the old file
            if (update)
            {
                File.Replace(Path.Combine(saveDir, rpiPicker.SelectedItem.ToString()) + ext, newFile, null);
                rpiPicker.Items[rpiPicker.SelectedIndex] = name;

                Send(RPiCmds.ClosePort);
                if (tcpClient != null)
                    tcpClient.Close();
                SetConnectionLabel("Not Connected", Color.Red);
            }

            else
            {
                rpiPicker.Items.Add(name);
            }
        }

        void AddDeviceContent(string name, int pin, bool save = true)
        {
            stackLayout.Children.Add(new DeviceContentView(name, pin, this));

            if (save)
                Save();
        }

        void AddDevicePgDisappearing(object sender, System.EventArgs e)
        {
            AddDevicePg adp = (AddDevicePg)sender;

            if (adp.WasSuccessful())
                AddDeviceContent(adp.GetDeviceName(), adp.GetPin());
        }

        void DeviceContentFinishedEditing(object sender, System.EventArgs e)
        {
            Save();
        }

        /// <summary>
        /// Saves the Device content data of the current rpi
        /// </summary>
        void Save()
        {
            if (rpiPicker.SelectedItem == null)
                return;

            //Overwrite file with current data
            using (StreamWriter sw = new StreamWriter(Path.Combine(saveDir, rpiPicker.SelectedItem.ToString()) + ext))
            {
                DeviceContentView dcv = null;
                for (int i = deviceContentOffset; i < stackLayout.Children.Count; i++)
                {
                    dcv = (DeviceContentView)stackLayout.Children[i];
                    sw.WriteLine($"{dcv.name} {dcv.pin}");
                }
            }
        }

        void Load()
        {
            if (rpiPicker.SelectedItem == null)
                return;

            using (StreamReader sr = new StreamReader(Path.Combine(saveDir, rpiPicker.SelectedItem.ToString()) + ext))
            {
                while (!sr.EndOfStream)
                {
                    string[] info = sr.ReadLine().Split(' ');
                    AddDeviceContent(info[0], int.Parse(info[1]), false);
                }
            }
        }

        void RpiPickerIndexChanged(object sender, System.EventArgs e)
        {
            ClearDeviceContentViews();
            if (rpiPicker.Items.Count == 0)
                return;

            Load();
        }

        /// <summary>
        /// Fills RPi picker with saved RPi names
        /// </summary>
        void PopulateRPiPicker()
        {
            string[] files = Directory.GetFiles(saveDir);
            for (int i = 0; i < files.Length; i++)
                rpiPicker.Items.Add(Path.GetFileNameWithoutExtension(files[i]));
        }

        /// <summary>
        /// Removes all device contents from page
        /// </summary>
        void ClearDeviceContentViews()
        {
            //Keep only the main elements and remove all device content of current rpi
            View[] a = new View[deviceContentOffset];
            for (int i = 0; i < deviceContentOffset; i++)
                a[i] = stackLayout.Children[i];

            stackLayout.Children.Clear();
            for (int i = 0; i < deviceContentOffset; i++)
                stackLayout.Children.Add(a[i]);
        }
    }
}