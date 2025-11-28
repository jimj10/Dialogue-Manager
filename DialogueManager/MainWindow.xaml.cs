using DialogueManager.EventLog;
using DialogueManager.ViewModels;
using DialogueManager.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogueManager
{

    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            AppSetup.MainWindow = this;
            if (!AppSetup.InitialiseApp())
            {
                this.Close();
            }

            MainMenu.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#e8940c"));
            var dashboardView = new DashboardView();
            TabMgr.MainWindow = this;
            Grid.SetRow(TabMgr.TabControl, 1);
            MainGrid.Children.Add(TabMgr.TabControl);
            TabMgr.AddOrSelectTabItem("Home", "HomeGrid", 1, dashboardView);
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            this.Closed += new EventHandler(MainWindowClosed);
        }

        void MainWindowClosed(object sender, EventArgs e)
        {
            Logger.AddLogEntry(LogCategory.INFO, String.Format("App Closing: Clearing temporary files..."));
            DirectoryInfo dinfo = new DirectoryInfo(DirectoryMgr.TempDirectory);
            foreach (FileInfo file in dinfo.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
