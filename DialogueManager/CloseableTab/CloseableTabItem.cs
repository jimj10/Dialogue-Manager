using System;
using System.Windows;
using System.Windows.Controls;

namespace DialogueManager.CloseableTab
{
    public class CloseableTabItem : TabItem
    {
        // use Guid as there may be multiple instances with same grid name

        public Guid tabId { get; set; }

        public Grid TabGrid { get; set; }

        public string TabName { get; set; }

        public UserControl UserCtrl1 { get; set; }

        public UserControl UserCtrl2 { get; set; }

        public UserControl UserCtrl3 { get; set; }

        public CloseableTabItem()
        {
            tabId = Guid.NewGuid();
        }

        public CloseableTabItem(TextBlock header, bool closeable = true)
        {
            tabId = Guid.NewGuid();
            header.FontSize = 14;
            header.VerticalAlignment = VerticalAlignment.Center;
            // Container for header controls

            var dockPanel = new DockPanel();
            dockPanel.Children.Add(header);

            // Close button to remove the tab
            if (closeable)
            {
                var closeButton = new TabCloseButton();
                closeButton.Click +=
                    (sender, e) =>
                    {
                        TabMgr.CloseTab(tabId);
                    };
                dockPanel.Children.Add(closeButton);
            }
            Header = dockPanel;
        }

        private void OnCloseBtnClick(object sender, RoutedEventArgs e)
        {
            TabMgr.CloseTab(tabId);
        }
    }
}
