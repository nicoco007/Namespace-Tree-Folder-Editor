using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace NamespaceTreeFolderEditor
{
    public static class DialogExtensions
    {
        public async static Task<MessageBoxResult> ShowMessageAsync(this MetroWindow window, string title, string message, MessageBoxButton textStyle)
        {
            var settings = new MetroDialogSettings()
            {
                AnimateShow = false,
                AnimateHide = false
            };

            var style = MessageDialogStyle.Affirmative;

            switch (textStyle)
            {
                case MessageBoxButton.OK:
                    settings.AffirmativeButtonText = "OK";
                    break;

                case MessageBoxButton.OKCancel:
                    settings.AffirmativeButtonText = "OK";
                    settings.NegativeButtonText = "Cancel";
                    style = MessageDialogStyle.AffirmativeAndNegative;
                    break;

                case MessageBoxButton.YesNo:
                    settings.AffirmativeButtonText = "Yes";
                    settings.NegativeButtonText = "No";
                    style = MessageDialogStyle.AffirmativeAndNegative;
                    break;

                case MessageBoxButton.YesNoCancel:
                    settings.AffirmativeButtonText = "Yes";
                    settings.NegativeButtonText = "No";
                    settings.FirstAuxiliaryButtonText = "Cancel";
                    style = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                    break;
            }

            switch (await window.ShowMessageAsync(title, message, style, settings))
            {
                case MessageDialogResult.Affirmative:
                    if (textStyle == MessageBoxButton.OK || textStyle == MessageBoxButton.OKCancel)
                        return MessageBoxResult.OK;
                    else if (textStyle == MessageBoxButton.YesNo || textStyle == MessageBoxButton.YesNoCancel)
                        return MessageBoxResult.Yes;
                    break;

                case MessageDialogResult.Negative:
                    if (textStyle == MessageBoxButton.OKCancel)
                        return MessageBoxResult.Cancel;
                    else if (textStyle == MessageBoxButton.YesNo || textStyle == MessageBoxButton.YesNoCancel)
                        return MessageBoxResult.No;
                    break;

                case MessageDialogResult.FirstAuxiliary:
                    return MessageBoxResult.Cancel;
            }

            return MessageBoxResult.OK;
        }
    }
}
