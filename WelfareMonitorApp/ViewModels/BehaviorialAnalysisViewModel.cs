using System.Collections.ObjectModel;
using Java.Util;
using WelfareMonitorApp.Services;

namespace WelfareMonitorApp.ViewModels;

public class BehaviorialAnalysisViewModel : BindableObject
{
    private readonly FirestoreService _firestoreService;
    private System.Timers.Timer _dataTimer;

    private bool _isLoading = true;

    public bool isLoading
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
    
    // ObservableCollection for the Picker
    public ObservableCollection<string> PigIds { get; set; } = new ObservableCollection<string>();

    private string _selectedPigId;

    public string selectedPigId
    {
        get => _selectedPigId;
        set
        {
            if (_selectedPigId != value)
            {
                _selectedPigId = value;
                OnPropertyChanged();
            }
        }
    }

    public BehaviorialAnalysisViewModel(FirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    public void StartDataRetrieval()
    {
        
    }
}