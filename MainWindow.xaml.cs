using LABB_3.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace LABB_3;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();
    private bool _hasSaved;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (sender, e) => await _viewModel.LoadAsync();
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_hasSaved) return;

        e.Cancel = true;                 // stop closing for now...
        await _viewModel.SaveAsync();    // ...wait until the file is saved...
        _hasSaved = true;
        Close();                         // ...then close for real
    }
}