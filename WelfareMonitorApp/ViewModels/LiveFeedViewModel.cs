//ViewModels/LiveFeedViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using WelfareMonitorApp.Services;
using Google.Cloud.Firestore;
using Microsoft.Maui.Controls;

namespace WelfareMonitorApp.ViewModels
{
    public class LiveFeedViewModel
    {
        private readonly FirestoreService _firestoreService;

        public ObservableCollection<Animal> Animals { get; set; } = new ObservableCollection<Animal>();

        public LiveFeedViewModel(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        public async Task AddAnimalDataAsync()
        {
            var data = new Dictionary<string, object>
            {
                { "name", "Lion" },
                { "species", "Panthera leo" },
                { "age", 5 },
                { "location", "Savannah" }
            };

            await _firestoreService.AddDataAsync("animals", data);
        }

        public async Task LoadAnimalsAsync()
        {
            var documents = await _firestoreService.GetDataAsync("animals");
            Animals.Clear();

            foreach (var doc in documents)
            {
                var animal = doc.ConvertTo<Animal>();
                Animals.Add(animal);
            }
        }
    }

    public class Animal
    {
        public string Name { get; set; }
        public string Species { get; set; }
        public int Age { get; set; }
        public string Location { get; set; }
    }
}
