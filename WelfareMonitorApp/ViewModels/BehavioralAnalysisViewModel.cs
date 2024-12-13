using System.Collections.ObjectModel;
using Android.Util;
using Microcharts;
using WelfareMonitorApp.Models;
using WelfareMonitorApp.Services;
using Exception = System.Exception;
using System.Linq;
using SkiaSharp;

namespace WelfareMonitorApp.ViewModels;

public class BehavioralAnalysisViewModel : BindableObject
{
    private readonly FirestoreService _firestoreService;
    
    // Loading state
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
                UpdateCharts();
                OnPropertyChanged();
            }
        }
    }
    
    // LTA Data
    private List<LongTermAnalysis> _ltaData = new List<LongTermAnalysis>();
    private List<LongTermAnalysis> _currentLtaData;
    
    // Loading flags
    private bool _isInitialDataLoaded = false;
    
    // Options
    public ObservableCollection<DropdownItem> Options { get; set; }
    private DropdownItem _selectedOption;
    public DropdownItem SelectedOption
    {
        get => _selectedOption;
        set
        {
            _selectedOption = value;
            UpdateCharts();
            OnPropertyChanged();
        }
    }

    // Chart
    private List<ChartEntry> _ChartEntries = new List<ChartEntry>();
    private Chart _LTAChart;
    public Chart LTAChart
    {
        get => _LTAChart;
        set
        {
            if (_LTAChart != value)
            {
                _LTAChart = value;
                OnPropertyChanged();
            }
        }
    }

    public BehavioralAnalysisViewModel(FirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
        
        Options = new ObservableCollection<DropdownItem>
        {
            new DropdownItem { Label = "Percentage Laying", Value = "percentageLaying" },
            new DropdownItem { Label = "Percentage Standing", Value = "percentageStanding" },
            new DropdownItem { Label = "Percentage Moving", Value = "percentageMoving" },
            new DropdownItem { Label = "Average Distance", Value = "avgDistance" },
            new DropdownItem { Label = "Total distance", Value = "totalDistance" },
            new DropdownItem { Label = "Average Confidence", Value = "avgConfidence" },
        };
        
        Task.Run(async () => await GetData());
    }

    private async Task GetData()
    {
        try
        {
            // Fetch data asynchronously
            _ltaData = await _firestoreService.GetLongTermAnalysisAsync();

            // Main thread updates
            Device.BeginInvokeOnMainThread(() =>
            {
                // Set default options and update UI
                SelectedOption ??= Options.FirstOrDefault();
                
                PigIds.Clear();
                
                foreach (var pigId in _ltaData.Select(lta => lta.PigId).Distinct())
                {
                    PigIds.Add(pigId);
                }
                
                if (PigIds.Any() && string.IsNullOrEmpty(SelectedPigId))
                {
                    SelectedPigId = PigIds.First();
                }
                
                UpdateCharts(); // Safe to call now
            });
        }
        catch (Exception ex)
        {
            Log.Error("GetData", $"Failed to fetch data: {ex.Message}");
        }
        finally
        {
            IsLoading = false; // Ensure loading state is updated
        }
    }

    private void UpdateCharts()
    {
        Log.Debug("UpdateCharts", "Updating charts...");
        if (string.IsNullOrEmpty(SelectedPigId) || _ltaData == null || SelectedOption == null)
        {
            Log.Error("UpdateCharts", "UpdateCharts was called with invalid state.");
            return;
        }

        // Filter so we only get lta data for the current pig
       _currentLtaData = _ltaData
           .Where(lta => lta.PigId == SelectedPigId)
           .OrderBy(lta => lta.timestamp)
           .ToList();
       
       IEnumerable<string> times = _currentLtaData.Select(lta => lta.timestamp);
       
       IEnumerable<bool> keeperPresent = _currentLtaData.Select(lta => lta.keeperPresent);
       
       // Determine the property to filter by based on SelectedOption
       IEnumerable<float> datapoints = Enumerable.Empty<float>();
       switch (SelectedOption.Value)
       {
           case "percentageLaying":
               datapoints = _currentLtaData.Select(lta => lta.percentageLaying);
               break;
           case "percentageStanding":
               datapoints = _currentLtaData.Select(lta => lta.percentageStanding);
               break;
           case "percentageMoving":
               datapoints = _currentLtaData.Select(lta => lta.percentageMoving);
               break;
           case "avgDistance":
               datapoints = _currentLtaData.Select(lta => lta.avgDistance);
               break;
           case "totalDistance":
               datapoints = _currentLtaData.Select(lta => lta.totalDistance);
               break;
           case "avgConfidence":
               datapoints = _currentLtaData.Select(lta => lta.avgConfidence);
               break;
           default:
               Log.Error("UpdateCharts", $"Unexpected SelectedOption.Value: {SelectedOption.Value}");
               return;
       }
       
       Log.Debug("UpdateCharts", $"Datapoints: {string.Join(", ", datapoints)}");
       
       Device.BeginInvokeOnMainThread(() =>
       {
           _ChartEntries.Clear();
           
           foreach (var (data, keeper, time) in Enumerable.Zip(datapoints, keeperPresent, times))
           {
               _ChartEntries.Add(new ChartEntry(data)
               {
                   Label = $"{DateTime.Parse(time).ToString("hh:mm:ss")}",
                   ValueLabel = $"{data:F2}",
                   Color = keeper ? SKColors.MediumSlateBlue : SKColors.Goldenrod,
               });
           }
           
           Log.Debug("UpdateCharts", $"Chart Entries: {string.Join(", ", _ChartEntries.Select(x => x.Value))}");
           
           LTAChart = new BarChart
           {
               Entries = _ChartEntries, 
               LabelTextSize = 32,
               LabelOrientation = Orientation.Horizontal, 
               ValueLabelTextSize = 32,
               ValueLabelOrientation = Orientation.Horizontal,
               LabelColor = SKColors.Black,
           };
       });

    }

    private void CheckIfLoadingComplete()
    {
        if (_isInitialDataLoaded)
        {
            IsLoading = false;
        }
    }

    public class DropdownItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

}