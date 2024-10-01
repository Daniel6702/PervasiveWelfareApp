using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;

namespace WelfareMonitorApp.Views 
{
    public partial class DashboardPage : ContentPage
    {
        private readonly LocationService _locationService;
        private bool _isRunning;

        public DashboardPage()
        {
            InitializeComponent();
            _locationService = new LocationService(this);
            StartLocationUpdates();
        }

        private void StartLocationUpdates()
        {
            _isRunning = true;
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (!_isRunning)
                    return false; 

                _ = UpdateLocationAsync();

                return true; 
            });
        }

        private async Task UpdateLocationAsync()
        {
            var location = await _locationService.GetCurrentLocationAsync();
            if (location != null)
            {
                string locationText = $"Latitude: {location.Latitude:F5}, Longitude: {location.Longitude:F5}";
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text = locationText;
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text = "Unable to retrieve location.";
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false;
        }
    }
}
