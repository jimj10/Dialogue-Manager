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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogueManager.Views
{
    public partial class SessionsAdminView : UserControl
    {

        private SessionsAdminViewModel viewModel;
        public SessionsAdminView()
        {
            InitializeComponent();
            viewModel = new SessionsAdminViewModel();
            DataContext = viewModel;
        }

        private void AudioClipBtnClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnAudioClipBtnClick(sender as Button);
        }

        private void SessionClipBtnClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnSessionClipBtnClick(sender as Button);
        }

        private void AudioClipMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button lblFrom = e.Source as Button;

            if (e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(lblFrom, lblFrom, DragDropEffects.Copy);
        }

        private void AudioClipQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            Button lblFrom = e.Source as Button;

            if (!e.KeyStates.HasFlag(DragDropKeyStates.LeftMouseButton))
                lblFrom.Content = "...";
        }

        private void SessionAudioClipsDrop(object sender, DragEventArgs e)
        {
            string draggedText = (string)e.Data.GetData(DataFormats.StringFormat);

            Button toLabel = e.Source as Button;
            toLabel.Content = draggedText;
        }
    }
}
