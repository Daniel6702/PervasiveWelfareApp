using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Helpers; 

namespace WelfareMonitorApp.Views 
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = ServiceProviderAccessor.Instance.GetService<DashboardViewModel>();
        }
        
    }
}