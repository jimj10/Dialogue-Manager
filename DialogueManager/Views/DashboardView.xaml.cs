/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogueManager.Views
{
    public partial class DashboardView : UserControl
    {  
        private readonly Regex NumericRegex = new Regex("[^0-9]+");

        private DashboardViewModel viewModel;

        public DashboardView()
        {
            InitializeComponent();
            viewModel = new DashboardViewModel();
            DataContext = viewModel; 
        }

        // Need to pass Button to DashboardViewModel methods
        private void AudioClipBtnClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnAudioClipBtnClick(sender as Button);
        }

        private void TimePreviewInput(object sender, TextCompositionEventArgs e)
        {
            // Prohibit non-numeric characters
            if (NumericRegex.IsMatch(e.Text))
                e.Handled = true;
        }

        private void HourTextBoxMouseLeave(object sender, RoutedEventArgs e)
        {
           HourTextBox.Text = viewModel.CheckHours(HourTextBox.Text);
        }

        private void MinuteTextBoxMouseLeave(object sender, RoutedEventArgs e)
        {
           MinuteTextBox.Text = viewModel.CheckMinutes(MinuteTextBox.Text);
        }

        private void OnMouseEnterPlayBtn(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.IsEnabled)
                viewModel.DisplayAudioText(btn.Tag.ToString());
        }
    }
}
