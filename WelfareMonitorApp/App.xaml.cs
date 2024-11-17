//App.xaml.cs
using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;
using WelfareMonitorApp.Helpers;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace WelfareMonitorApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new LoadingPage();
    }
}
