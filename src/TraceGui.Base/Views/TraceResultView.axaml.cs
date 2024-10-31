using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TraceGui.Views;

public partial class TraceResultView : UserControl
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

