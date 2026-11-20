using Avalonia.Controls;

namespace Avalo;

/// <summary>Диалог подтверждения удаления.</summary>
public partial class ConfirmWindow : Window
{
    public ConfirmWindow()
    {
        InitializeComponent();
    }

    public ConfirmWindow(string message) : this()
    {
        MessageText.Text = message;
    }

    private void OnYes(object? sender, RoutedEventArgs e) => Close(true);

    private void OnNo(object? sender, RoutedEventArgs e) => Close(false);
}
