using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TraceGui.Views;

public class TraceResultView : UserControl
{
    public TraceResultView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

