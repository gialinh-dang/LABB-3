using System.Windows;
using LABB_3.Dialogs;
using LABB_3.ViewModel;

namespace LABB_3.Services
{
    // The only place that opens windows. ViewModels call these methods
    // instead of creating windows themselves.
    public static class DialogService
    {
        public static bool ShowPackOptionsDialog(MainWindowViewModel viewModel)
        {
            return ShowDialog(new PackOptionsDialog(), viewModel);
        }

        public static bool ShowNewPackDialog(MainWindowViewModel viewModel)
        {
            return ShowDialog(new CreateNewPackDialog(), viewModel);
        }

        public static void ShowImportDialog(ImportViewModel viewModel)
        {
            var dialog = new ImportQuestionsDialog();
            dialog.Loaded += async (sender, e) => await viewModel.LoadCategoriesAsync();
            ShowDialog(dialog, viewModel);
        }

        public static bool Confirm(string message)
        {
            var result = MessageBox.Show(message, "Math Quiz", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Math Quiz", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void CloseMainWindow()
        {
            Application.Current.MainWindow.Close();
        }

        private static bool ShowDialog(Window dialog, object dataContext)
        {
            dialog.DataContext = dataContext;
            dialog.Owner = Application.Current.MainWindow;
            return dialog.ShowDialog() == true;
        }
    }
}
