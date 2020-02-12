/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using System;
using System.Windows;
using System.Windows.Controls;

namespace DialogueManager.Views
{

    public partial class MessageDialogWin : Window
    {
        public event EventHandler<string> MessageResponse;

        public MessageDialogWin(string title, string message, string btnLabel1, string btnLabel2)
        {
            InitializeComponent();
            MsgWin.Title = title;
            Message.Text = message;
            Btn1.Content = btnLabel1;
            Btn2.Content = btnLabel2;
        }

        public void SetText(string message)
        {
            Message.Text = message;
        }

        private void OnBtnClick(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            MessageResponse(this, content);
        }
    }
}
