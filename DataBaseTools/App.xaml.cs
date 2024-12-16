using DataBaseTools.ViewModels;
using Prism.Ioc;
using Prism.Unity;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DataBaseTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
        }
    }

}
