using WelfareMonitorApp.ViewModels;

namespace WelfareMonitorApp.Views;

public partial class QueuesPage : ContentPage
{
	public QueuesPage(QueuesViewModel queuesViewModel)
	{
        this.InitializeComponent();
        this.BindingContext = queuesViewModel;
	}
}