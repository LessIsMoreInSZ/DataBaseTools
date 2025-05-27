using DataBaseTools.net48.ViewModels;
using DataBaseTools.net48.ViewModels.Dialogs;
using DataBaseTools.net48.Views;
using Prism.Ioc;
using Prism.Unity;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;


namespace DataBaseTools.net48
{
    /// <summary>
    /// App.xaml 的交互逻辑
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
            //containerRegistry.RegisterForNavigation<SqliteView, SqliteViewModel>();
            containerRegistry.RegisterDialog<MessageView, MessageViewModel>();


        }
    }
}
