using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalo;

/// <summary>Диалог причины отмены (минимум 10 символов).</summary>
public partial class CancelReasonWindow : Window
{
    public string Reason { get; private set; } = "";

    public CancelReasonWindow()
    {
        InitializeComponent();
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        string reason = ReasonBox.Text?.Trim() ?? "";
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
