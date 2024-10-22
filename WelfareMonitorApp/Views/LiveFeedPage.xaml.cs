// Views/LiveFeedPage.xaml.cs
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Helpers; // Add this using directive

namespace WelfareMonitorApp.Views 
{
    public partial class LiveFeedPage : ContentPage
    {
        public LiveFeedPage()
        {
            InitializeComponent();
            // Resolve the ViewModel from the ServiceProviderAccessor
            BindingContext = ServiceProviderAccessor.Instance.GetService<LiveFeedViewModel>();
        }
    }
}
