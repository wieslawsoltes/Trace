using Avalonia.Web.Blazor;

namespace TraceGui.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<TraceGui.App>()
            .SetupWithSingleViewLifetime();
    }
}
