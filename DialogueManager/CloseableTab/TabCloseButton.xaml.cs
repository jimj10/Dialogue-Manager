using System;
using System.Windows;
using System.Windows.Controls;

namespace DialogueManager.CloseableTab
{
    public partial class TabCloseButton : UserControl
    {
        public event EventHandler Click;

        public TabCloseButton()
        {
            InitializeComponent();
        }

        private void OnCloseBtnClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}