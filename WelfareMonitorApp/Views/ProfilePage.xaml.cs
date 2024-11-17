// ProfilePage.xaml.cs
using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
