/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.CloseableTab;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DialogueManager
{
    static class TabMgr
    {
        public static Window MainWindow { get; set; } // Main app window; used to position other windows

        private static List<Grid> Grids = new List<Grid>();
        private static List<CloseableTabItem> TabItems = new List<CloseableTabItem>();

        public static TabControl TabControl = new TabControl { Width = 1200, Height = 620 };

        public static bool AddOrSelectTabItem(string header, string gridName, int columns, UserControl uc1, UserControl uc2 = null)
        {
            // Add tab item if it doesn't already exist
            CloseableTabItem tab = TabItems.Find(i => i.TabGrid.Name.Equals(gridName));
            if (tab == null)
            {
                AddNewTabItem(header, gridName, columns, uc1, uc2);
                return true;
            }
            TabControl.SelectedItem = tab;
            return false;
        }

        public static void AddNewTabItem(string header, string gridName, int columns,
            UserControl uc1, UserControl uc2 = null)
        {
            // Add tab item (whether or not another instance exists)
            var headerText = new TextBlock { Text = header };
            var tab = gridName.Equals("HomeGrid") ? new CloseableTabItem(headerText, false) : new CloseableTabItem(headerText);
            tab.TabName = header;
            tab.UserCtrl1 = uc1;
            tab.UserCtrl2 = uc2;
            var grid = GetGrid(gridName, columns);
            tab.TabGrid = grid;
            Grid.SetColumn(uc1, 0);
            grid.Children.Add(uc1);
            if (uc2 != null)
            {
                Grid.SetColumn(uc2, 1);
                grid.Children.Add(uc2);
            }
            tab.Content = grid;
            TabControl.Items.Add(tab);
            TabItems.Add(tab);
            TabControl.SelectedItem = tab;
        }

        public static void UpdateTabItem(string gridName, int column, UserControl uc)
        {
            var tab = TabItems.Find(i => i.TabGrid.Name.Equals(gridName));
            if (tab != null)
            {
                if (column == 0 && tab.UserCtrl1 != uc)
                {
                    tab.TabGrid.Children.Remove(tab.UserCtrl1);
                    if (uc != null)
                    {
                        Grid.SetColumn(uc, 0);
                        tab.TabGrid.Children.Add(uc);
                        tab.UserCtrl1 = uc;
                    }
                }
                else if (column == 1 && tab.UserCtrl2 != uc)
                {
                    tab.TabGrid.Children.Remove(tab.UserCtrl2);
                    if (uc != null)
                    {
                        Grid.SetColumn(uc, 1);
                        tab.TabGrid.Children.Add(uc);
                        tab.UserCtrl2 = uc;
                    }
                }
            }
            TabControl.SelectedItem = tab;
            EventSystem.Publish<TabChanged>(new TabChanged
            {
                TabName = tab.TabName,
                UserCtrlType = uc.GetType().ToString()
            });
        }

        public static void CloseTab(Guid tabId)
        {
            var tab = TabItems.Find(i => i.tabId == tabId);
            if (tab != null)
            {
                TabControl.Items.Remove(tab);
                if (tab.UserCtrl1 != null)
                    tab.TabGrid.Children.Remove(tab.UserCtrl1);
                if (tab.UserCtrl2 != null)
                    tab.TabGrid.Children.Remove(tab.UserCtrl2);
                TabItems.Remove(tab);
                tab = null;
            }
        }

        private static Grid GetGrid(string name, int numberOfColumns)
        {
            Grid grid = new Grid();
            grid.Name = name;
            RowDefinition row0 = new RowDefinition()
            {
                Height = new GridLength(600, GridUnitType.Pixel)
            };
            grid.RowDefinitions.Add(row0);
            if (numberOfColumns == 2)
            {
                ColumnDefinition column0 = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star),
                };
                ColumnDefinition column1 = new ColumnDefinition()
                {
                    Width = new GridLength(2, GridUnitType.Star),
                };
                grid.ColumnDefinitions.Add(column0);
                grid.ColumnDefinitions.Add(column1);
            }
            Grids.Add(grid);
            return grid;
        }
    }
}
