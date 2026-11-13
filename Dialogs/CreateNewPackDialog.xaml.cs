using System.Windows;
using System.Windows.Input;

namespace LABB_3.Dialogs;

public partial class CreateNewPackDialog : Window
{
    public CreateNewPackDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;   // closes the dialog and tells the ViewModel "Save"
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();   // the window has no title bar, so we drag it from the header
    }
}
