using Xamarin.Forms;
using Urho;
using Urho.Forms;
using Urho.Resources;
using MotionPlayground.ViewModels;
using MotionPlayground3D;

namespace MotionPlayground
{
    public partial class MainPage : ContentPage
    {
        Playground3DApp _app;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _app = await UrhoView.Show<Playground3DApp>(new ApplicationOptions("Data"));

            if (BindingContext is MainViewModel vm)
            {
                vm.Engine3D = _app;
                vm.StopCommand.Execute(null);
                vm.StartCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as MainViewModel)?.StopCommand.Execute(null);
        }

        private void OnAnimationsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
