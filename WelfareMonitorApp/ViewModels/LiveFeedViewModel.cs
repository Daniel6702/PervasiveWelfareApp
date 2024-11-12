using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        private System.Timers.Timer _imageTimer;
        private System.Timers.Timer _dataTimer;

        // New Property for Loading State
        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if(_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

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
                    UpdateCurrentMovementData();
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

        private MovementData _currentMovementData;
        public MovementData CurrentMovementData
        {
            get => _currentMovementData;
            set
            {
                _currentMovementData = value;
                OnPropertyChanged();
            }
        }

        private CurrentBehavoir _currentBehavoir = new CurrentBehavoir();

        public CurrentBehavoir CurrentBehavoir
        {
            get => _currentBehavoir;
            set
            {
                _currentBehavoir = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _pigImageUrls = new Dictionary<string, string>();
        private Dictionary<string, MovementData> _pigMovementData = new Dictionary<string, MovementData>();

        // Flags to check if initial data is loaded
        private bool _isInitialImageLoaded = false;
        private bool _isInitialDataLoaded = false;

        public LiveFeedViewModel(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
            StartDataRetrieval();
        }

        public void StartDataRetrieval()
        {
            // Timer for images (every 10 seconds)
            _imageTimer = new System.Timers.Timer(10000); // 10 seconds
            _imageTimer.Elapsed += async (sender, e) => await RetrieveImagesAsync();
            _imageTimer.Start();
            // Initial image retrieval
            Task.Run(async () => await RetrieveImagesAsync());

            // Timer for movement data (every 1 second)
            _dataTimer = new System.Timers.Timer(1000); // 1 second
            _dataTimer.Elapsed += async (sender, e) => await RetrieveMovementDataAsync();
            _dataTimer.Start();
            // Initial data retrieval
            Task.Run(async () => await RetrieveMovementDataAsync());
        }

        private async Task RetrieveImagesAsync()
        {
            try
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

                // Set initial image loaded flag
                if (!_isInitialImageLoaded)
                {
                    _isInitialImageLoaded = true;
                    CheckIfLoadingComplete();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display error)
                Console.WriteLine($"Error retrieving images: {ex.Message}");
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
                Console.WriteLine($"Error updating image: {ex.Message}");
            }
        }

        private async Task RetrieveMovementDataAsync()
        {
            try
            {
                List<MovementData> movementDataList = await _firestoreService.GetMovementDataAsync();

                // Update the pigMovementData dictionary
                lock (_pigMovementData)
                {
                    _pigMovementData = movementDataList.ToDictionary(md => md.PigId, md => md);
                }

                // Update the current movement data if necessary
                if (!string.IsNullOrEmpty(SelectedPigId))
                {
                    UpdateCurrentMovementData();
                }

                // Set initial data loaded flag
                if (!_isInitialDataLoaded)
                {
                    _isInitialDataLoaded = true;
                    CheckIfLoadingComplete();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display error)
                Console.WriteLine($"Error retrieving movement data: {ex.Message}");
            }
        }

        private void UpdateCurrentMovementData()
        {
            if (string.IsNullOrEmpty(SelectedPigId))
                return;

            MovementData movementData;
            lock (_pigMovementData)
            {
                if (!_pigMovementData.TryGetValue(SelectedPigId, out movementData))
                    return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentMovementData = movementData;
                interpret_movement_data(movementData);
            });
        }

        private void interpret_movement_data(MovementData movementData)
        {

            Console.WriteLine("Interpreting movement data 1");
            if (movementData == null)
            {
                return;
            }
            Console.WriteLine("Interpreting movement data 2");

            _currentBehavoir.PigId = movementData.PigId;

            if (movementData.M1 == 0) {
                _currentBehavoir.Behavior = "Unknown";
            } else if (movementData.M1 == 1) {
                _currentBehavoir.Behavior = "Laying";
            } else if (movementData.M1 == 2) {
                _currentBehavoir.Behavior = "Standing";
            } else if (movementData.M1 == 3) {
                _currentBehavoir.Behavior = "Moving";
            }

            _currentBehavoir.Distance = movementData.Distance;

            int seconds = movementData.LastWalking;
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int remainingSeconds = seconds % 60;
            string readableTime = $"{hours} t, {minutes} m, {remainingSeconds} s";
            _currentBehavoir.LastWalking = readableTime;

            if (movementData.PigClassObjectDetect == 1) {
                _currentBehavoir.PigClassObjectDetect = "Laying";
            } else {
                _currentBehavoir.PigClassObjectDetect = "Standing";
            }

            if (movementData.KeeperPresenceObjectDetect == 1) {
                _currentBehavoir.KeeperPresenceObjectDetect = true;
            } else {
                _currentBehavoir.KeeperPresenceObjectDetect = false;
            }

            CurrentBehavoir = _currentBehavoir;

            Console.WriteLine("Interpreting movement data 3");

            Console.WriteLine("PigId: " + _currentBehavoir.PigId);
        }

        // Check if both initial image and data are loaded
        private void CheckIfLoadingComplete()
        {
            if (_isInitialImageLoaded && _isInitialDataLoaded)
            {
                IsLoading = false;
            }
        }

        public void StopDataRetrieval()
        {
            _imageTimer?.Stop();
            _imageTimer?.Dispose();
            _dataTimer?.Stop();
            _dataTimer?.Dispose();
        }

    }
}
