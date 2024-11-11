using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using WelfareMonitorApp.Services;
using System.Timers;
using WelfareMonitorApp.Models;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace WelfareMonitorApp.ViewModels
{
    public class LiveFeedViewModel : BindableObject
    {
        private readonly FirestoreService _firestoreService;
        private System.Timers.Timer _timer;

        // ObservableCollection for the Picker
        public ObservableCollection<string> PigIds { get; set; } = new ObservableCollection<string>();

        private string _selectedPigId;
        public string SelectedPigId
        {
            get => _selectedPigId;
            set
            {
                if (_selectedPigId != value)
                {
                    _selectedPigId = value;
                    OnPropertyChanged();
                    UpdateCurrentImage();
                }
            }
        }

        private ImageSource _currentImage;
        public ImageSource CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _pigImageUrls = new Dictionary<string, string>();

        public LiveFeedViewModel(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
            StartDataRetrieval();
        }

        public void StartDataRetrieval()
        {
            _timer = new System.Timers.Timer(10000); // 10 seconds
            _timer.Elapsed += async (sender, e) => await RetrieveImagesAsync();
            _timer.Start();
            // Initial retrieval
            Task.Run(async () => await RetrieveImagesAsync());
        }

        private async Task RetrieveImagesAsync()
        {
            List<PigImage> pigImages = await _firestoreService.GetPigImagesAsync();

            // Extract the new list of PigIds
            var newPigIds = pigImages.Select(pi => pi.PigId).Distinct().ToList();

            // Update the PigIds collection on the main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                // Remove PigIds that are no longer in the new list
                for (int i = PigIds.Count - 1; i >= 0; i--)
                {
                    if (!newPigIds.Contains(PigIds[i]))
                    {
                        PigIds.RemoveAt(i);
                    }
                }

                // Add new PigIds that are not already in the collection
                foreach (var pigId in newPigIds)
                {
                    if (!PigIds.Contains(pigId))
                    {
                        PigIds.Add(pigId);
                    }
                }

                // Automatically select the first Pig if no Pig is currently selected
                if (string.IsNullOrEmpty(SelectedPigId) && PigIds.Count > 0)
                {
                    SelectedPigId = PigIds.First();
                }
            });

            // Update the pigImageUrls dictionary
            lock (_pigImageUrls)
            {
                _pigImageUrls = pigImages.ToDictionary(pi => pi.PigId, pi => pi.ImageUrl);
            }

            // Update the current image if necessary
            if (!string.IsNullOrEmpty(SelectedPigId))
            {
                await UpdateCurrentImage();
            }
        }

        private async Task UpdateCurrentImage()
        {
            if (string.IsNullOrEmpty(SelectedPigId))
                return;

            string imageUrl;
            lock (_pigImageUrls)
            {
                if (!_pigImageUrls.TryGetValue(SelectedPigId, out imageUrl))
                    return;
            }

            try
            {
                using HttpClient httpClient = new HttpClient();
                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);
                // Convert byte array to ImageSource
                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageData));
                Device.BeginInvokeOnMainThread(() =>
                {
                    CurrentImage = imageSource;
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display error)
            }
        }
    }
}
