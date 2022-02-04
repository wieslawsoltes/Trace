using System;
using System.IO;
using System.Linq;
using BitmapToVector;
using BitmapToVector.SkiaSharp;
using SkiaSharp;

namespace TraceCli
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: TraceCli <filename> [filename.svg]");
                return;
            }
            var inputFileName = args[0];
            var outputFileName = args.Length == 1 ? Path.ChangeExtension(inputFileName, ".svg") : args[1];
            using var source = SKBitmap.Decode(inputFileName);
            var param = new PotraceParam();
            var paths = PotraceSkiaSharp.Trace(param, source).ToList();
            SvgWriter.Save(source.Width, source.Height, paths, outputFileName);
        }
    }
}
