using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace TraceGui.Controls
{
    public class GeometryCanvas : Control
    {
        public static readonly StyledProperty<IEnumerable<PathGeometry>?> PathsProperty = 
            AvaloniaProperty.Register<GeometryCanvas, IEnumerable<PathGeometry>?>(nameof(Paths));

        public IEnumerable<PathGeometry>? Paths
        {
            get => GetValue(PathsProperty);
            set => SetValue(PathsProperty, value);
        }

        static GeometryCanvas()
        {
            AffectsRender<GeometryCanvas>(PathsProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var paths = Paths;
            
            if (paths is not null)
            {
                var brush = new ImmutableSolidColorBrush(Color.Parse("#000000"));

                foreach (var path in paths)
                {
                    context.DrawGeometry(brush, null, path);
                }
            }
        }
    }
}