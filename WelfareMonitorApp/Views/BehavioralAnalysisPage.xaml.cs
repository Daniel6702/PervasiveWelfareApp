using Microsoft.Maui.Controls;
using WelfareMonitorApp.Helpers;
using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views 
{
    public partial class BehavioralAnalysisPage : ContentPage
    {
        public BehavioralAnalysisPage()
        {
            InitializeComponent();
            BindingContext = ServiceProviderAccessor.Instance.GetService<BehaviorialAnalysisViewModel>();
        }
    }
}
