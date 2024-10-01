using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;
using WelfareMonitorApp.Models;
using System.Collections.ObjectModel;

namespace WelfareMonitorApp.Views 
{
    public partial class DashboardPage : ContentPage
    {
        private readonly LocationService _locationService;
        private bool _isRunning;

        public ObservableCollection<Pig> Pigs { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            _locationService = new LocationService(this);
            StartLocationUpdates();

            Pigs = new ObservableCollection<Pig>
            {
                new Pig { Id = 1, Name = "Pig 1", Status = "Healthy", BehavioralSummary = "Active, eating regularly", CurrentActivity = "Eating" },
                new Pig { Id = 2, Name = "Pig 2", Status = "Caution", BehavioralSummary = "Inactive for 4 hours", CurrentActivity = "Resting" },
                new Pig { Id = 3, Name = "Pig 3", Status = "Alert", BehavioralSummary = "Not drinking for 12 hours", CurrentActivity = "Standing" }
            };

            // Set the BindingContext for the page
            BindingContext = this;
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
