using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace NamespaceTreeFolderEditor
{
    public partial class EditDialog : MetroWindow
    {
        RootFolder initialFolder = null;

        public EditDialog()
        {
            InitializeComponent();
        }

        public new void ShowDialog()
        {
            Title = (initialFolder != null ? "Edit" : "Add") + " a folder";
            base.ShowDialog();
        }

        public void ShowDialog(RootFolder rootFolder)
        {
            initialFolder = rootFolder;

            if (rootFolder != null)
            {
                enabledCheckBox.IsChecked = rootFolder.Enabled;
                nameTextBox.Text = rootFolder.Name;
                pathTextBox.Text = rootFolder.Path;
                indexNumUpDown.Value = rootFolder.Index;
                iconPathTextBox.Text = rootFolder.IconPath;
                iconIndexNumUpDown.Value = rootFolder.IconIndex;
                showOnDesktopCheckBox.IsChecked = rootFolder.ShowOnDesktop;
                
                pathTextBox.IsEnabled = string.IsNullOrEmpty(rootFolder.TargetKnownFolder);
                pathButton.IsEnabled = string.IsNullOrEmpty(rootFolder.TargetKnownFolder);
            }

            ShowDialog();
        }

        private void pathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderSelectDialog();

            if (dialog.ShowDialog())
            {
                pathTextBox.Text = dialog.FileName;
            }
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextBox.Text))
            {
                await ShowErrorMessage("Please enter a valid name.");
            }
            else if (string.IsNullOrEmpty(pathTextBox.Text) && initialFolder?.TargetKnownFolder == null)
            {
                await ShowErrorMessage("Please enter a path.");
            }
            else if (!Directory.Exists(pathTextBox.Text) && initialFolder?.TargetKnownFolder == null)
            {
                await ShowErrorMessage("The selected path does not exist.");
            }
            else if (!(indexNumUpDown.Value >= 0))
            {
                await ShowErrorMessage("List location must be a positive number.");
            }
            else if (string.IsNullOrEmpty(iconPathTextBox.Text))
            {
                await ShowErrorMessage("Please enter an icon path.");
            }
            else if (!File.Exists(iconPathTextBox.Text))
            {
                await ShowErrorMessage("The selected icon file does not exist.");
            }
            else if (!(iconIndexNumUpDown.Value >= 0))
            {
                await ShowErrorMessage("Icon index must be a positive number.");
            }
            else
            {
                RootFolder folder = new RootFolder(
                    initialFolder?.Guid,
                    nameTextBox.Text,
                    !string.IsNullOrEmpty(pathTextBox.Text) ? pathTextBox.Text : null,
                    initialFolder?.TargetKnownFolder,
                    (int?)indexNumUpDown.Value ?? 0,
                    iconPathTextBox.Text,
                    (int?)iconIndexNumUpDown.Value ?? 0,
                    showOnDesktopCheckBox.IsChecked ?? false,
                    enabledCheckBox.IsChecked ?? false
                );

                RootFolder.AddFolder(folder);

                Close();
            }
        }

        private async Task ShowErrorMessage(string message)
        {
            await this.ShowMessageAsync("Error", message, MessageBoxButton.OK);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void iconPathButton_Click(object sender, RoutedEventArgs e)
        {
            IconPicker iconPicker = new IconPicker();

            iconPicker.FileName = iconPathTextBox.Text;
            iconPicker.IconIndex = (int?)iconIndexNumUpDown.Value ?? 0;

            if (iconPicker.ShowDialog())
            {
                iconPathTextBox.Text = iconPicker.FileName;
                iconIndexNumUpDown.Value = iconPicker.IconIndex;
            }
        }
    }
}
