using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MotionPlayground
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.Transparent,
                BarTextColor = Color.White
            };
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
