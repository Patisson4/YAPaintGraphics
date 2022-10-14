using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YAPaint.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void Exit(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
