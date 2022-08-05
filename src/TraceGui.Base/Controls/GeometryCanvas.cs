using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace TraceGui.Controls;

public class GeometryCanvas : Control
{
    public static readonly StyledProperty<IEnumerable<PathGeometry>?> PathsProperty = 
        AvaloniaProperty.Register<GeometryCanvas, IEnumerable<PathGeometry>?>(nameof(Paths));

    public static readonly StyledProperty<IBrush> BrushProperty = 
        AvaloniaProperty.Register<GeometryCanvas, IBrush>(nameof(Brush), new SolidColorBrush(Color.Parse("#000000")));

    public IEnumerable<PathGeometry>? Paths
    {
        get => GetValue(PathsProperty);
        set => SetValue(PathsProperty, value);
    }

    public IBrush Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    static GeometryCanvas()
    {
        AffectsRender<GeometryCanvas>(PathsProperty, BrushProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var paths = Paths;
            
        if (paths is not null)
        {
            var brush = Brush;

            foreach (var path in paths)
            {
                context.DrawGeometry(brush, null, path);
            }
        }
    }
}