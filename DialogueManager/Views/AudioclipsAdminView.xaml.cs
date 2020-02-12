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

namespace DialogueManager.Views
{

    public partial class AudioclipsAdminView : UserControl
    {

        private AudioclipsAdminViewModel viewModel;

        public AudioclipsAdminView()
        {
            InitializeComponent();
            viewModel = new AudioclipsAdminViewModel
            {
                LightBlueSquare = LightBlueBtn,
                BeigeSquare = BeigeBtn,
                OrangeSquare = OrangeBtn,
                LightGreenSquare = LightGreenBtn
            };
            DataContext = viewModel;
        }

        private void AudioClipBtnClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnAudioClipBtnClick((sender as Button));
        }

        private void OnColourBtnClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnColourBtnClick(sender as Button);
        }

    }
}

