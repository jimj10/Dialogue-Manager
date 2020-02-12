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
    public partial class MessageWin : Window
    {
        public MessageWin(string title, string message)
        {
            InitializeComponent();
            MsgWin.Title = title;
            Message.Text = message;
        }

        public void AppendText(string newText)
        {
            string updatedMsg = Message.Text + "\n" + newText;
            Message.Text = updatedMsg;
        }
    }
}
