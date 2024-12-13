using Microcharts;
using Microcharts.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp;
using WelfareMonitorApp.Helpers;
using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views 
{
    public partial class BehavioralAnalysisPage : ContentPage
    {
        public BehavioralAnalysisPage()
        {
            InitializeComponent();
            BindingContext = ServiceProviderAccessor.Instance.GetService<BehavioralAnalysisViewModel>();
        }
    }
}
