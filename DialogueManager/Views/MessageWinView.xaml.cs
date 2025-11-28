/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using System.Windows;

namespace DialogueManager.Views
{

    public partial class MessageWinView : Window
    {
        public string Title
        {
            get { return MsgWin.Title; }
            set { MsgWin.Title = value; }
        }

        public MessageWinView(string title, string message)
        {
            InitializeComponent();
            MsgWin.Title = title;
            Message.Text = message;
            this.Loaded += new RoutedEventHandler(WindowLoaded);
        }

        public MessageWinView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WindowLoaded);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Position at centre of app window
            this.Top = AppSetup.MainWindow.Top + (AppSetup.MainWindow.ActualHeight / 2) - this.ActualHeight / 2;
            this.Left = AppSetup.MainWindow.Left + (AppSetup.MainWindow.ActualWidth / 2) - this.ActualWidth / 2;
        }

        public void SetText(string newText)
        {
            Message.Text = newText;
        }

        public void AppendText(string newText)
        {
            string updatedMsg = Message.Text + "\n" + newText;
            Message.Text = updatedMsg;
        }
    }
}
