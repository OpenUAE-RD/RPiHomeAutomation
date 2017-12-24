using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace AutoRPi
{
    public partial class App : Application
    {
        MainPage mainpg;

        public App()
        {
            InitializeComponent();

            mainpg = new MainPage();
            MainPage = new NavigationPage(mainpg) { BarBackgroundColor = Color.DarkSlateBlue};
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            mainpg.AppClosed();
        }

        protected override void OnResume()
        {
            mainpg.AppResumed();
        }
    }
}
