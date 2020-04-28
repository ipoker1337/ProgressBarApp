using System.Windows;
using ProgressApp.Wpf.ViewModels;

namespace ProgressApp.Wpf {

public partial class 
App : Application {
    private void Application_Startup(object sender, StartupEventArgs e) {
        var mainWindow = new MainWindow { DataContext = new MainViewModel() };
        mainWindow.Show();
    }
}
}
