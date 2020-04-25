using System.Windows;
using WpfApp2.ViewModels;

namespace WpfApp2 {

public partial class 
App : Application {
    private void Application_Startup(object sender, StartupEventArgs e) {
        var mainWindow = new MainWindow { DataContext = new MainViewModel() };
        mainWindow.Show();
    }
}
}
