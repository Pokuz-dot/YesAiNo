using Avalonia.Controls;

namespace Allure.Desktop.Views;

/// <summary>Модальный диалог ввода причины отмены (минимум 10 символов).</summary>
public partial class CancelReasonWindow : Window
{
    public string Reason { get; private set; } = string.Empty;

    public CancelReasonWindow()
    {
        InitializeComponent();
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        string reason = ReasonBox.Text?.Trim() ?? string.Empty;
        if (reason.Length < 10)
        {
            ErrorText.Text = "Причина должна содержать не менее 10 символов.";
            return;
        }
        Reason = reason;
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
