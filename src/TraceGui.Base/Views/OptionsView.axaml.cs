using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TraceGui.Views;

public class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

