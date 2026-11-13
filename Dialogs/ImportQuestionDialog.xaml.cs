using System.Windows;
using System.Windows.Input;

namespace LABB_3.Dialogs;

public partial class ImportQuestionsDialog : Window
{
    public ImportQuestionsDialog()
    {
        InitializeComponent();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
