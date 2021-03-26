using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform;
using BitmapToVector;
using SkiaSharp;

namespace TraceGui
{
    public static class PotraceAvalonia
    {
        private static IPlatformRenderInterface? _factory;

        private static IPlatformRenderInterface Factory => _factory ??= AvaloniaLocator.Current.GetService<IPlatformRenderInterface>();

        public static IEnumerable<IGeometryImpl> Trace(PotraceParam param, SKBitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var (bytesPerPixel, bytesOffset) = GetBytesInfo(bitmap.ColorType);
            var pixelsIntPtr = bitmap.GetPixels();
            PotraceState traceResult;
            unsafe
            {
                var ptr = (byte*)pixelsIntPtr.ToPointer() + bytesOffset;
                
                using (var potraceBitmap = PotraceBitmap.Create(width, height))
                {
                    for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        // For speed, only check 1 byte for the pixel.
                        // This is the red color if the ColorType has a red component.
                        if (*ptr < 128)
                        {
                            potraceBitmap.SetBlackUnsafe(x, y);
                        }

                        ptr += bytesPerPixel;
                    }

                    traceResult = Potrace.Trace(param, potraceBitmap);
                }
            }

            return CreateSKPaths(traceResult.Plist);
        }

        public static IEnumerable<IGeometryImpl > Trace(PotraceParam param, PotraceBitmap potraceBitmap)
        {
            var traceResult = Potrace.Trace(param, potraceBitmap);
            return CreateSKPaths(traceResult.Plist);
        }

        private static IEnumerable<IGeometryImpl > CreateSKPaths(PotracePath rootPath)
        {
            var pathGroups = GetPathGroups(rootPath);
            foreach (var pathGroup in pathGroups)
            {
                var geometry =  Factory.CreateStreamGeometry();
                using var context = geometry.Open();

                foreach (var potracePath in pathGroup)
                {
                    var potraceCurve = potracePath.Curve;
            
                    var lastPoint = potraceCurve.C[potraceCurve.N - 1][2];
                    context.BeginFigure(new Point((float) lastPoint.X, (float) lastPoint.Y));

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

        private static (int BytesPerPixel, int Offset) GetBytesInfo(SKColorType colorType)
        {
            switch (colorType)
            {
                case SKColorType.Alpha8:
                    return (BytesPerPixel: 1, Offset: 0);
                case SKColorType.Rgba8888:
                    return (BytesPerPixel: 4, Offset: 0);
                case SKColorType.Rgb888x:
                    return (BytesPerPixel: 4, Offset: 0);
                case SKColorType.Bgra8888:
                    return (BytesPerPixel: 4, Offset: 2);
                case SKColorType.Gray8:
                    return (BytesPerPixel: 1, Offset: 0);
                case SKColorType.Rg88:
                    return (BytesPerPixel: 2, Offset: 0);

                case SKColorType.Unknown:
                case SKColorType.Rgb565:
                case SKColorType.Argb4444:
                case SKColorType.Rgba1010102:
                case SKColorType.Rgb101010x:
                case SKColorType.RgbaF16:
                case SKColorType.RgbaF16Clamped:
                case SKColorType.RgbaF32:
                case SKColorType.AlphaF16:
                case SKColorType.RgF16:
                case SKColorType.Alpha16:
                case SKColorType.Rg1616:
                case SKColorType.Rgba16161616:
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(SKColorType)} {colorType} is not supported");
            }
        }
    }
}