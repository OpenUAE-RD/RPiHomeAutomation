using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AutoRPi
{
    public class AddRPiPg : ContentPage
    {
        Entry en;
        Button btn;
        string oldName;
        public Action<string, bool> rpiAddedCallback;
        public readonly IList<string> usedNames;

        bool Update { get { return oldName != string.Empty; } }

        public AddRPiPg(IList<string> usedNames, string oldName = "")
        {
            this.oldName = oldName;
            this.usedNames = usedNames;

            en = new Entry() { Text = oldName, Placeholder = "Raspberry Pi Name", PlaceholderColor = Color.LightGray, HorizontalOptions = LayoutOptions.FillAndExpand };
            btn = new Button() { Text = Update ? "Update Name" : "Add RPi", BackgroundColor = Color.LightGreen, HorizontalOptions = LayoutOptions.FillAndExpand };
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
            string n = en.Text.Trim();
            if (n == string.Empty)
            {
                DisplayAlert("Error", "Please enter the name of your Raspberry Pi", "OK");
                return;
            }

            if (n.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                DisplayAlert("Error", "Invalid name. Please avoid using special characters.", "OK");
                return;
            }

            if (n != oldName && usedNames.Contains(n))
            {
                DisplayAlert("Error", "Raspberry Pi name is already used.", "OK");
                return;
            }

            rpiAddedCallback?.Invoke(n, Update);
            Navigation.PopAsync(true);
        }
    }
}