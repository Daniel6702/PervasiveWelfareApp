using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views;

public partial class LogPage : ContentPage
{
    public LogPage(LogViewModel logViewModel)
    {
        this.InitializeComponent();
        this.BindingContext = logViewModel;
    }
}