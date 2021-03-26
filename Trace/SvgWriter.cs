using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;

namespace Trace
{
    static class SvgWriter
    {
        public static string ToSvg(int width, int height, IEnumerable<SKPath> paths, string fillColor)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\">");

            foreach (var path in paths)
            {
                sb.AppendLine($"    <path fill=\"{fillColor}\" d=\"{path.ToSvgPathData()}\"/>");
            }

            sb.AppendLine($"</svg>");

            return sb.ToString();
        }

        public static void Save(int width, int height, IEnumerable<SKPath> paths, string filename, string fillColor = "#000000")
        {
            var svg = ToSvg(width, height, paths, fillColor);
            File.WriteAllText(filename, svg);
        }
    }
}