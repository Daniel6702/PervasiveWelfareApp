using Microsoft.Maui.Controls;
using WelfareMonitorApp.Views;
using WelfareMonitorApp.Helpers;

namespace WelfareMonitorApp
{
    public partial class AppShell : Shell
    {
         private readonly IServiceProvider _serviceProvider;


        public AppShell()
        {
            InitializeComponent();

            _serviceProvider = ServiceProviderAccessor.Instance;

            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        }

		private async void OnProfileButtonClicked(object sender, EventArgs e)
        {
            var profilePage = _serviceProvider.GetService(typeof(ProfilePage)) as ProfilePage;
            await Navigation.PushAsync(profilePage);
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

    }
}
