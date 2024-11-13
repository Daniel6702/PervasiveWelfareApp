using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui.Controls;
using WelfareMonitorApp.Models;
using WelfareMonitorApp.Services;

namespace WelfareMonitorApp.ViewModels
{
    public class DashboardViewModel : BindableObject
    {
        private readonly FirestoreService _firestoreService;
        private System.Timers.Timer _timer;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        // ObservableCollection to hold the welfare logs
        public ObservableCollection<WelfareLog> WelfareLogs { get; set; } = new ObservableCollection<WelfareLog>();

        public DashboardViewModel(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
            StartDataRetrieval();
        }

        private void StartDataRetrieval()
        {
            // Timer to periodically retrieve data every 10 seconds
            _timer = new System.Timers.Timer(10000); // 10 seconds
            _timer.Elapsed += async (sender, e) => await RetrieveWelfareDataAsync();
            _timer.Start();

            // Initial data retrieval
            Task.Run(async () => await RetrieveWelfareDataAsync());
        }

        private async Task RetrieveWelfareDataAsync()
        {
            try
            {
                List<WelfareLog> welfareLogs = await _firestoreService.GetWelfareDataAsync();

                if (welfareLogs != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // Clear the existing collection
                        WelfareLogs.Clear();

                        // Add the latest welfare logs
                        foreach (var log in welfareLogs)
                        {
                            WelfareLogs.Add(log);
                        }

                        IsLoading = false;
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display error)
                Console.WriteLine($"Error retrieving welfare data: {ex.Message}");
            }
        }

        public void StopDataRetrieval()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
