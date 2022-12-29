using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using YAPaint.ViewModels;

namespace YAPaint.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel(this);
        InitializeComponent();
    }

    private void Exit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ColorSpacesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        foreach (var checkBox in comboBox.GetLogicalSiblings().OfType<CheckBox>())
        {
            checkBox.IsVisible = MainWindowViewModel.ThreeChannelColorSpaceNames.Contains(comboBox?.SelectedItem);
        }
    }

    private void InputElement_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }
}
