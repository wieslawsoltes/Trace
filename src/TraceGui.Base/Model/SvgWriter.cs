using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace TraceGui.Model;

static class SvgWriter
{
    public static string ToSvg(int width, int height, IEnumerable<PathGeometry> paths, string fillColor)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\">");

        foreach (var path in paths)
        {
            var d = path.ToString();
            sb.AppendLine($"    <path fill=\"{fillColor}\" d=\"{d}\"/>");
        }

        sb.AppendLine($"</svg>");

        return sb.ToString();
    }

    public static async Task Save(int width, int height, IEnumerable<PathGeometry> paths, Stream stream, string fillColor = "#000000")
    {
        var svg = ToSvg(width, height, paths, fillColor);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(svg);
    }
}
