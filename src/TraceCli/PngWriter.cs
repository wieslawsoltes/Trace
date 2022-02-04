using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace TraceCli
{
    static class PngWriter
    {
        public static SKBitmap ToBitmap(int width, int height, IEnumerable<SKPath> paths, string fillColor)
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = SKColor.Parse(fillColor)
            };

            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            foreach (var path in paths)
            {
                canvas.DrawPath(path, paint);
            }

            return bitmap;
        }

        public static void Save(int width, int height, IEnumerable<SKPath> paths, string filename, string fillColor = "#000000")
        {
            using var bitmap = ToBitmap(width, height, paths, fillColor);
            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var file = File.Create(filename);
            data.SaveTo(file);
        }
    }
}
