using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;

namespace TraceGui
{
    public class GeometryCanvas : Control
    {
        public static readonly StyledProperty<IEnumerable<IGeometryImpl>?> PathsProperty = 
            AvaloniaProperty.Register<GeometryCanvas, IEnumerable<IGeometryImpl>?>(nameof(Paths));

        public IEnumerable<IGeometryImpl>? Paths
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
                    context.PlatformImpl.DrawGeometry(brush, null, path);
                }
            }
        }
    }
}