using WelfareMonitorApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace WelfareMonitorApp
{
    public partial class MainPage : ContentPage
    {
        private readonly LocationService _locationService;

        public MainPage()
        {
            InitializeComponent();
            _locationService = new LocationService();
            CheckPermissions();
        }

        private async void CheckPermissions()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Location permission is required to access GPS.", "OK");
            }
        }

        private async void GetLocation_Clicked(object sender, EventArgs e)
        {
            var location = await _locationService.GetCurrentLocationAsync();

            if (location != null)
            {
                await DisplayAlert("Location", $"Latitude: {location.Latitude}, Longitude: {location.Longitude}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Unable to retrieve the location.", "OK");
            }
        }
    }
}
