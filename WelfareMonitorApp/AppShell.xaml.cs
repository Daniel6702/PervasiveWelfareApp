using Microsoft.Maui.Controls;
using WelfareMonitorApp.Views;

namespace WelfareMonitorApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

		private async void OnProfileButtonClicked(object sender, EventArgs e)
		{
			await Current.GoToAsync("//profile");
		}

    }
}
