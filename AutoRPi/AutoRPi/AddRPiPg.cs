using System;
using Xamarin.Forms;

namespace AutoRPi
{
    public class AddRPiPg : ContentPage
    {
        public Action<string> rpiAddedCallback;
        Entry en;
        Button btn;

        public AddRPiPg()
        {
            en = new Entry() { Placeholder = "Raspberry Pi Name", PlaceholderColor = Color.LightGray, HorizontalOptions = LayoutOptions.FillAndExpand };
            btn = new Button() { Text = "Add RPi", BackgroundColor = Color.LightGreen, HorizontalOptions = LayoutOptions.FillAndExpand };
            btn.Clicked += ButtonClicked;

            Content = new StackLayout
            {
                Children =
                {
                    en,
                    btn
                }
            };
        }

        void ButtonClicked(object sender, EventArgs e)
        {
            if (en.Text.Trim() == string.Empty)
            {
                DisplayAlert("Error", "Please enter the name of your Raspberry Pi", "OK");
                return;
            }

            rpiAddedCallback?.Invoke(en.Text.Trim());
            Navigation.PopAsync(true);
        }
    }
}