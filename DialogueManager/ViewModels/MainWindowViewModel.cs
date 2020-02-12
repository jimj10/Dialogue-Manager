/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using DialogueManager.EventLog;
using DialogueManager.Views;
using System.Windows.Input;

namespace DialogueManager.ViewModels
{
    class MainWindowViewModel
    {
        private SessionsAdminView sessionsAdminView;
        private AudioclipsAdminView audioclipsAdminView;
        private SettingsView settingsView;
        private LogViewerCtrl logViewerCtrl;
        private AboutWindow aboutWindow;

        public MainWindowViewModel()
        {

        }

        private ICommand audioclipsMenuClick;
        public ICommand AudioclipsMenuClick {
            get {
                audioclipsMenuClick = audioclipsMenuClick ?? new RelayCommand(param => DisplayAudioclipsAdminView(param), param => true);
                return audioclipsMenuClick;
            }
            set { audioclipsMenuClick = value; }
        }

        private void DisplayAudioclipsAdminView(object obj)
        {
            audioclipsAdminView = audioclipsAdminView ?? new AudioclipsAdminView();
            TabMgr.AddOrSelectTabItem("Audio clips", "AudiclipsAdminGrid", 1, audioclipsAdminView);
            
        }

        private ICommand sessionsMenuClick;
        public ICommand SessionsMenuClick {
            get {
                sessionsMenuClick = sessionsMenuClick ?? new RelayCommand(param => DisplaySessionsAdminView(param), param => true);
                return sessionsMenuClick;
            }
            set { sessionsMenuClick = value; }
        }

        private void DisplaySessionsAdminView(object obj)
        {
            sessionsAdminView = sessionsAdminView ?? new SessionsAdminView();
            TabMgr.AddOrSelectTabItem("Sessions", "SessionsAdminGrid", 1, sessionsAdminView);
        }

        private ICommand settingsMenuClick;
        public ICommand SettingsMenuClick {
            get {
                settingsMenuClick = settingsMenuClick ?? new RelayCommand(param => DisplaySettingsView(param), param => true);
                return settingsMenuClick;
            }
            set { settingsMenuClick = value; }
        }

        private void DisplaySettingsView(object obj)
        {
            settingsView = settingsView ?? new SettingsView();
            TabMgr.AddOrSelectTabItem("Settings", "SettingsGrid", 1, settingsView);
        }

        private ICommand eventLogMenuClick;
        public ICommand EventLogMenuClick {
            get {
                eventLogMenuClick = eventLogMenuClick ?? new RelayCommand(param => DisplayEventLog(param), param => true);
                return eventLogMenuClick;
            }
            set { settingsMenuClick = value; }
        }

        private void DisplayEventLog(object obj)
        {
            logViewerCtrl = logViewerCtrl ?? Logger.LogViewerCtrl;
            TabMgr.AddOrSelectTabItem("Event Log", "LogGrid", 1, logViewerCtrl);
        }

        private ICommand aboutMenuClick;
        public ICommand AboutMenuClick {
            get {
                aboutMenuClick = aboutMenuClick ?? new RelayCommand(param => DisplayAboutWindow(param), param => true);
                return aboutMenuClick;
            }
            set { aboutMenuClick = value; }
        }

        private void DisplayAboutWindow(object obj)
        {
            if (aboutWindow != null)
                aboutWindow.Close();
            aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

    }
}
