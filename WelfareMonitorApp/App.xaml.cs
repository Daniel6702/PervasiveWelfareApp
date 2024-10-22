using WelfareMonitorApp.Services;
 
namespace WelfareMonitorApp;

public partial class App : Application
{
    public static FirestoreService FirestoreServiceInstance { get; private set; }

    public App(FirestoreService firestoreService)
    {
        InitializeComponent();
        FirestoreServiceInstance = firestoreService;
        MainPage = new AppShell();
    }

    public static async Task InitializeFirestoreAsync(string projectId)
    {
        FirestoreServiceInstance = await FirestoreService.CreateAsync(projectId);
    }
}
