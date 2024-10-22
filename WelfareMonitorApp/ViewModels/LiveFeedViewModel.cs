// ViewModels/LiveFeedViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using WelfareMonitorApp.Services;
using Google.Cloud.Firestore;

namespace WelfareMonitorApp.ViewModels
{
    public class LiveFeedViewModel : BindableObject
    {
        private readonly FirestoreService _firestoreService;

        private ObservableCollection<Animal> _animals;
        public ObservableCollection<Animal> Animals
        {
            get => _animals;
            set
            {
                _animals = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDataCommand { get; }

        public LiveFeedViewModel(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
            Animals = new ObservableCollection<Animal>();
            LoadDataCommand = new Command(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var documents = await _firestoreService.GetDataAsync("animal_data");
                Animals.Clear();
                foreach (var doc in documents)
                {
                    if (doc.Exists)
                    {
                        var data = doc.ConvertTo<Animal>();
                        Animals.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    // Define the Animal model
    [FirestoreData]
    public class Animal
    {
        [FirestoreProperty("animal_id")]
        public string AnimalId { get; set; }

        [FirestoreProperty("image_url")]
        public string ImageUrl { get; set; }

        [FirestoreProperty("notes")]
        public string Notes { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; }

        [FirestoreProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
