using System;
using System.IO;
using System.Linq;
using BitmapToVector;
using BitmapToVector.SkiaSharp;
using SkiaSharp;

namespace Trace
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: Trace <filename> [filename.svg]");
                return;
            }

            var inputFileName = args[0];
            var outputFileName = args.Length == 2 ? Path.ChangeExtension(inputFileName, ".svg") : args[1];

            using var source = SKBitmap.Decode(inputFileName);
            var param = new PotraceParam();
            var paths = PotraceSkiaSharp.Trace(param, source).ToList();

            Converter.SaveAsSvg(source, paths, outputFileName);
        }
    }
}