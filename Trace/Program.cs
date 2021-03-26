using System;
using System.IO;
using System.Linq;
using BitmapToVector;
using BitmapToVector.SkiaSharp;
using SkiaSharp;

namespace Trace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Trace <filename>");
                return;
            }

            var filename = args[0];

            using var source = SKBitmap.Decode(filename);
            var param = new PotraceParam();
            var paths = PotraceSkiaSharp.Trace(param, source).ToList();

            Converter.SaveAsSvg(source, paths, Path.ChangeExtension(filename, ".svg"));
        }
    }
}