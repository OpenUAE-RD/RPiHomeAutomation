using Xamarin.Forms;

namespace AutoRPi
{
    public class DeviceContentView : ContentView
    {
        public string name { get; private set; }
        public int pin { get; private set; }
        Button edit, delete;
        Switch pinSwitch;
        MainPage mainPg;

        public DeviceContentView(string name, int pin, MainPage mainPg)
        {
            this.name = name;
            this.pin = pin;
            this.mainPg = mainPg;

            //Setup controls
            pinSwitch = new Switch() { HorizontalOptions = LayoutOptions.FillAndExpand };
            pinSwitch.Toggled += SwitchToggled;

            edit = new Button() { Text = "Edit", BackgroundColor = Color.DeepSkyBlue };
            edit.Clicked += EditClicked;

            delete = new Button() { Text = "Delete", BackgroundColor = Color.Red };
            delete.Clicked += DeleteClicked;

            Update(name, pin);
        }

        void SwitchToggled(object sender, System.EventArgs e)
        {
            mainPg.SwitchToggled(pin, pinSwitch.IsToggled);
        }

        void EditClicked(object sender, System.EventArgs e)
        {
            mainPg.EditBtnClicked(name, pin, this);
        }

        void DeleteClicked(object sender, System.EventArgs e)
        {
            mainPg.DeleteBtnClicked(this);
        }

        public void Update(string name, int pin)
        {
            this.name = name;
            this.pin = pin;

            //Assign content
            Content = (
                new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label() { Text = $"'{name}' on pin: {pin}", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Center,
                            VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.Black, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label))},
                        pinSwitch,
                        edit,
                        delete
                    }
                }
            );
        }
    }
}