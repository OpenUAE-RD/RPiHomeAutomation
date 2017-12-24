using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AutoRPi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddDevicePg : ContentPage
    {
        /// <summary>The device content view that was edited</summary>
        public DeviceContentView dcv;
        /// <summary>Whether the user successfully added a device</summary>
        bool success;

        string startName = "";
        int startPin = -1;

        public AddDevicePg(string name = "", int pin = -1, DeviceContentView dcv = null)
        {
            InitializeComponent();

            deviceNameEntry.Text = name;
            startName = name;

            startPin = pin;
            this.dcv = dcv;

            //Fill with GPIO pin numbers
            int[] pins = new int[40];
            for (int i = 0; i < 40; i++)
                pins[i] = i + 1;

            picker.ItemsSource = pins;
            picker.SelectedIndex = pin <= -1 ? -1 : pin - 1;
        }

        void AddDeviceBtn_Clicked(object sender, EventArgs e)
        {
            //Make sure we have required info
            if (deviceNameEntry.Text.Trim() == string.Empty)
            {
                DisplayAlert("Error", "Please input a device name", "OK");
                success = false;
                return;
            }

            if (picker.SelectedIndex == -1)
            {
                DisplayAlert("Error", "Please select a GPIO pin", "OK");
                success = false;
                return;
            }

            success = true;

            //If we are updating then update the device content
            if (dcv != null && (startName != GetDeviceName() || startPin != GetPin()))
                dcv.Update(GetDeviceName(), GetPin());

            Navigation.PopAsync(true);
        }

        public bool WasSuccessful()
        {
            return success;
        }

        public string GetDeviceName()
        {
            return deviceNameEntry.Text.Trim();
        }

        public int GetPin()
        {
            return (int)picker.ItemsSource[picker.SelectedIndex];
        }
    }
}