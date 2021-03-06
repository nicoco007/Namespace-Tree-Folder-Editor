﻿using MahApps.Metro.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NamespaceTreeFolderEditor
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {

            LoadList();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            showEditDialog();
        }

        private async void removeButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Are you sure?", "Are you sure you want to delete this folder?", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            foreach (var item in listView.SelectedItems.Cast<NamespaceTreeFolder>().ToArray())
            {
                item.Remove();
                listView.Items.Remove(item);
            }
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count == 1)
                showEditDialog((NamespaceTreeFolder)listView.SelectedItem);
        }

        private void LoadList()
        {
            listView.Items.Clear();

            var folders = NamespaceTreeFolder.GetFolders();
            folders.Sort((a, b) =>
            {
                if (b.Index == a.Index && a.Name != null && b.Name != null)
                    return a.Name.CompareTo(b.Name);
                else
                    return a.Index - b.Index;
            });

            foreach (var folder in folders)
                listView.Items.Add(folder);
        }

        private void listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listView.SelectedItems.Count == 1)
                showEditDialog((NamespaceTreeFolder)listView.SelectedItem);
        }

        private void showEditDialog(NamespaceTreeFolder rootFolder = null)
        {
            var dialog = new EditDialog();
            dialog.ShowDialog(rootFolder);
            LoadList();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeButton.IsEnabled = listView.SelectedItems.Count > 0;
            editButton.IsEnabled = listView.SelectedItems.Count == 1;
        }
    }
}
