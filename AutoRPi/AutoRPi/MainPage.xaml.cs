﻿using System.Collections.Generic;
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

        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        byte[] GetBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        void connectBtn_Clicked(object sender, System.EventArgs e)
        {
            ScanForRPi();
        }

        void SendBtn_Clicked(object sender, System.EventArgs e)
        {
            Send(rpiNameEntry.Text);
        }

        void UpdateConnectionStatus()
        {
            //TODO: Handle dropped connection
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
    }
}