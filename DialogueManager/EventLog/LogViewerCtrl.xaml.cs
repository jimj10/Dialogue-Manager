/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 * The Event Log code code is derived from code submitted to Stackoverflow by
 * Federico Berasategui (thank-you)
 * https://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf 
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace DialogueManager.EventLog
{

    public partial class LogViewerCtrl : UserControl, INotifyPropertyChanged
    {

        private bool displayLatestFirst = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogEntry> LogEntries { get; }

        public LogViewerCtrl()
        {
            InitializeComponent();
            DataContext = LogEntries = new ObservableCollection<LogEntry>();
        }

        public void AddLogEntry(LogEntry logEntry)
        {
            if (displayLatestFirst)
                Dispatcher.BeginInvoke((Action)(() => LogEntries.Insert(0, logEntry)));
            else
                Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(logEntry)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogEntries)));
        }
    }
}
