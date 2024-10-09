//LiveFeedPage.xaml.cs
using Microsoft.Maui.Controls;
using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views 
{
    public partial class LiveFeedPage : ContentPage
    {
        public LiveFeedPage(LiveFeedViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
