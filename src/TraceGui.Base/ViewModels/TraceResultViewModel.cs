#define USE_DYNAMIC_LINQ
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Avalonia.Media;
using BitmapToVector;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp.PixelFormats;
using TraceGui.Model;
#if !USE_DYNAMIC_LINQ
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
#endif

namespace TraceGui.ViewModels;

public partial class TraceResultViewModel : ViewModelBase, IDisposable
{
    private readonly SixLabors.ImageSharp.Image<Rgba32>? _source;
    [ObservableProperty] private string? _fileName;
    [ObservableProperty] private int _width;
    [ObservableProperty] private int _height;
    [ObservableProperty] private IEnumerable<PathGeometry>? _paths;
    [ObservableProperty] private string _fillColor = "#000000";

    public TraceResultViewModel()
    {
    }

    public TraceResultViewModel(SixLabors.ImageSharp.Image<Rgba32> source, string fileName)
    {
        _source = source;
        _width = _source.Width;
        _height = _source.Height;
        _fileName = fileName;
    }

    public async Task SaveStream(Stream stream)
    {
        if (_paths is not null)
        {
            await SvgWriter.Save(Width, Height, _paths, stream, _fillColor);
        }
    }

    public static async Task<SixLabors.ImageSharp.Image<Rgba32>> OpenStream(Stream stream)
    {
        return await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(stream);
    }

    public async Task Trace(OptionsViewModel options)
    {
        if (_source is null)
        {
            return;
        }

        var param = new PotraceParam()
        {
            TurdSize = options.TurdSize,
            TurnPolicy = options.TurnPolicy,
            AlphaMax = options.AlphaMax,
            OptiCurve = options.OptiCurve,
            OptTolerance = options.OptTolerance,
            QuantizeUnit = options.QuantizeUnit
        };

        static bool DefaultFilter(Rgba32 c) => c.R < 128 && c.A > 0;
        var filter = DefaultFilter;

        try
        {
            var compiledFilter = await Compile(options.Filter);
            filter = compiledFilter;
        }
        catch
        {
            Console.WriteLine("Failed to compile user filter.");
        }

        var paths = PotraceAvalonia.Trace(param, _source, filter).ToList();

        Paths = paths;
        FillColor = _fillColor;
    }

    public static async Task<Func<Rgba32, bool>> Compile(string filter)
    {
#if USE_DYNAMIC_LINQ
        return await Task.Run(() =>
        {
            var code = $"c => {filter}";
            Expression<Func<Rgba32, bool>> e = DynamicExpressionParser.ParseLambda<Rgba32, bool>(
                new ParsingConfig(),
                true,
                code);
            var compiledFilter = e.Compile();
            return compiledFilter;
        });
#else
        var code = $"c => {filter}";
        var options = ScriptOptions.Default.WithReferences(typeof(Rgba32).Assembly);
        var compiledFilter = await CSharpScript.EvaluateAsync<Func<Rgba32, bool>>(code, options);
        return compiledFilter;
#endif
    }

    public void Dispose()
    {
        _source?.Dispose();
    }
}
