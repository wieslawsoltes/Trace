using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Visuals.Platform;
using BitmapToVector;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = Avalonia.Point;

namespace TraceGui
{
    public static class PotraceAvalonia
    {
        public static IEnumerable<PathGeometry> Trace(PotraceParam param, Image<Rgba32> image, Func<Rgba32, bool> filter)
        {
            var width = image.Width;
            var height = image.Height;

            using var potraceBitmap = PotraceBitmap.Create(width, height);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var c = image[x, y];
                if (filter(c))
                {
                    potraceBitmap.SetBlackUnsafe(x, y);
                }
            }

            var traceResult = Potrace.Trace(param, potraceBitmap);

            return CreatePathGeometries(traceResult.Plist);
        }

        public static IEnumerable<PathGeometry> Trace(PotraceParam param, PotraceBitmap potraceBitmap)
        {
            var traceResult = Potrace.Trace(param, potraceBitmap);
            return CreatePathGeometries(traceResult.Plist);
        }

        private static IEnumerable<PathGeometry> CreatePathGeometries(PotracePath rootPath)
        {
            var pathGroups = GetPathGroups(rootPath);
            foreach (var pathGroup in pathGroups)
            {
                var geometry = new PathGeometry();
                using var context = new PathGeometryContext(geometry);

                foreach (var potracePath in pathGroup)
                {
                    var potraceCurve = potracePath.Curve;

                    var lastPoint = potraceCurve.C[potraceCurve.N - 1][2];
                    context.BeginFigure(new Point((float) lastPoint.X, (float) lastPoint.Y), true);

                    for (var i = 0; i < potraceCurve.N; i++)
                    {
                        var tag = potraceCurve.Tag[i];
                        var segment = potraceCurve.C[i];
                        if (tag == PotraceCurve.PotraceCorner)
                        {
                            var firstPoint = segment[1];
                            var secondPoint = segment[2];
                            context.LineTo(new Point((float) firstPoint.X, (float) firstPoint.Y));
                            context.LineTo(new Point((float) secondPoint.X, (float) secondPoint.Y));
                        }
                        else
                        {
                            var handle1 = segment[0];
                            var handle2 = segment[1];
                            var potracePoint = segment[2];
                            context.CubicBezierTo(
                                new Point((float) handle1.X, (float) handle1.Y),
                                new Point((float) handle2.X, (float) handle2.Y),
                                new Point((float) potracePoint.X, (float) potracePoint.Y));
                        }
                    }

                    context.EndFigure(true);
                }

                yield return geometry;
            }
        }

        private static IEnumerable<IEnumerable<PotracePath>> GetPathGroups(PotracePath rootPath)
        {
            var currentPath = rootPath;
            while (currentPath != null)
            {
                yield return GetNextGroup();
            }

            IEnumerable<PotracePath> GetNextGroup()
            {
                yield return currentPath;

                currentPath = currentPath.Next;
                while (currentPath?.Sign == '-')
                {
                    yield return currentPath;
                    currentPath = currentPath.Next;
                }
            }
        }
    }
}