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

namespace DialogueManager.Views
{

    public partial class DisplayWinView : Window
    {
        public DisplayWinView()
        {
            InitializeComponent();
        }

        public void SetText(string newText)
        {
            var lines = newText.Split(new[] { '\r', '\n' });
            Title.Text = lines[0];
            Body.Text = String.Empty;
            for (int i = 1; i < lines.Length; i++)
            {
                Body.Inlines.Add(lines[i] + "\n");
            }
        }
    }
}
