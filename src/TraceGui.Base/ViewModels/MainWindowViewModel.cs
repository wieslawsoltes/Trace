using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BitmapToVector;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ReactiveUI;
using SixLabors.ImageSharp.PixelFormats;
using TraceGui.Model;

namespace TraceGui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private SixLabors.ImageSharp.Image<Rgba32>? _source;
    private string? _fileName;
    private IEnumerable<PathGeometry>? _paths;
    private int _width;
    private int _height;
    private int _turdSize = 2;
    private int _turnPolicy = 4;
    private double _alphaMax = 1.0;
    private bool _optiCurve = true;
    private double _optTolerance = 0.2;
    private uint _quantizeUnit = 10;
    private string _filter = "c.R < 128 && c.A > 0";
    private string _fillColor = "#000000";

    public MainWindowViewModel()
    {
        OpenCommand = ReactiveCommand.CreateFromTask(async () => await OnOpen());
        SaveCommand = ReactiveCommand.CreateFromTask(async () => await OnSave());

        this.WhenAnyValue(x => x.TurdSize).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.TurnPolicy).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.AlphaMax).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.OptiCurve).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.OptTolerance).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.QuantizeUnit).Subscribe(_ => Trace());
        this.WhenAnyValue(x => x.Filter).Subscribe(_ => Trace());

        Task.Run(async () =>
        {
            try
            {
                await Compile(_filter);
            }
            catch
            {
                Debug.WriteLine("Failed to compile user filter.");
            }
        });
    }

    public ICommand OpenCommand { get; }

    public ICommand SaveCommand { get; }

    public string? FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    public IEnumerable<PathGeometry>? Paths
    {
        get => _paths;
        set => this.RaiseAndSetIfChanged(ref _paths, value);
    }

    public int Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    public int Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    public int TurdSize
    {
        get => _turdSize;
        set => this.RaiseAndSetIfChanged(ref _turdSize, value);
    }
        
    public int TurnPolicy
    {
        get => _turnPolicy;
        set => this.RaiseAndSetIfChanged(ref _turnPolicy, value);
    }
        
    public double AlphaMax
    {
        get => _alphaMax;
        set => this.RaiseAndSetIfChanged(ref _alphaMax, value);
    }
        
    public bool OptiCurve
    {
        get => _optiCurve;
        set => this.RaiseAndSetIfChanged(ref _optiCurve, value);
    }

    public double OptTolerance
    {
        get => _optTolerance;
        set => this.RaiseAndSetIfChanged(ref _optTolerance, value);
    }

    public uint QuantizeUnit
    {
        get => _quantizeUnit;
        set => this.RaiseAndSetIfChanged(ref _quantizeUnit, value);
    }

    public string Filter
    {
        get => _filter;
        set => this.RaiseAndSetIfChanged(ref _filter, value);
    }

    public string FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }

    private List<FilePickerFileType> GetOpenFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.ImageAll,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetSaveFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.ImageSvg,
            StorageService.All
        };
    }
    
    private async Task OnOpen()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open image",
            FileTypeFilter = GetOpenFileTypes(),
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null && file.CanOpenRead)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                Load(stream, file.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    private async Task OnSave()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save drawing",
            FileTypeChoices = GetSaveFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension(_fileName),
            DefaultExtension = "svg",
            ShowOverwritePrompt = true
        });

        if (file is not null && file.CanOpenWrite)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                await Save(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    private async Task Save(Stream stream)
    {
        if (_paths is not null)
        {
            await SvgWriter.Save(Width, _height, _paths, stream, FillColor);
        }
    }

    public void Load(Stream stream, string filename)
    {
        Decode(stream);

        Trace();

        FileName = filename;
    }

    private void Decode(Stream stream)
    {
        _source?.Dispose();
        _source = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
    }

    private async void Trace()
    {
        if (_source is null)
        {
            return;
        }

        var param = new PotraceParam()
        {
            TurdSize = _turdSize,
            TurnPolicy = _turnPolicy,
            AlphaMax = _alphaMax,
            OptiCurve = _optiCurve,
            OptTolerance = _optTolerance,
            QuantizeUnit = _quantizeUnit
        };

        static bool DefaultFilter(Rgba32 c) => c.R < 128;
        var filter = DefaultFilter;

        try
        {
            var compiledFilter = await Compile(_filter);
            filter = compiledFilter;
        }
        catch
        {
            Debug.WriteLine("Failed to compile user filter.");
        }

        var paths = PotraceAvalonia.Trace(param, _source, filter).ToList();

        Width = _source.Width;
        Height = _source.Height;
        Paths = paths;
    }

    private async Task<Func<Rgba32, bool>> Compile(string filter)
    {
        var code = $"c => {filter}";
        var options = ScriptOptions.Default.WithReferences(typeof(Rgba32).Assembly);
        var compiledFilter = await CSharpScript.EvaluateAsync<Func<Rgba32, bool>>(code, options);
        return compiledFilter;
    }
}
