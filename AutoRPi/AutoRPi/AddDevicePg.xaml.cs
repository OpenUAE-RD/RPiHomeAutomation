using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AutoRPi
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddDevicePg : ContentPage
	{
        bool success;

		public AddDevicePg()
		{
			InitializeComponent();

            //Fill with GPIO pin numbers
            int[] pins = new int[40];
            for (int i = 0; i < 40; i++)
                pins[i] = i + 1;

            picker.SelectedIndex = -1;
            picker.ItemsSource = pins;
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
            Navigation.PopAsync();
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