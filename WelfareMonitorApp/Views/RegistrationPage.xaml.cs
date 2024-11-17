// RegistrationPage.xaml.cs
using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views
{
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage(RegistrationViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
