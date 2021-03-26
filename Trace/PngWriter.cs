using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace Trace
{
    static class PngWriter
    {
        public static void Save(SKBitmap source, IEnumerable<SKPath> paths, string filename, string fillColor = "#000000")
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = SKColor.Parse(fillColor)
            };

            using var bitmap = new SKBitmap(source.Width, source.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            foreach (var path in paths)
            {
                canvas.DrawPath(path, paint);
            }

            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var file = File.Create(filename);

            data.SaveTo(file);
        }
    }
}