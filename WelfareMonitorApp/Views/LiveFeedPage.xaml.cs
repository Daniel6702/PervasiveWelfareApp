// Views/LiveFeedPage.xaml.cs
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Helpers; 

namespace WelfareMonitorApp.Views 
{
    public partial class LiveFeedPage : ContentPage
    {
        public LiveFeedPage()
        {
            InitializeComponent();
            BindingContext = ServiceProviderAccessor.Instance.GetService<LiveFeedViewModel>();
        }
    }
}
