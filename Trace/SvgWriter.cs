using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;

namespace Trace
{
    static class SvgWriter
    {
        public static void Save(SKBitmap source, IEnumerable<SKPath> paths, string filename, string fillColor = "#000000")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{source.Width}\" height=\"{source.Height}\">");

            foreach (var path in paths)
            {
                sb.AppendLine($"    <path fill=\"{fillColor}\" d=\"{path.ToSvgPathData()}\"/>");
            }

            sb.AppendLine($"</svg>");

            var svg = sb.ToString();

            File.WriteAllText(filename, svg);
        }
    }
}