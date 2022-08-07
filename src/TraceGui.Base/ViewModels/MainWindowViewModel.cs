using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BitmapToVector;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ReactiveMarbles.PropertyChanged;
using SixLabors.ImageSharp.PixelFormats;
using TraceGui.Model;

namespace TraceGui.ViewModels;

[ObservableObject]
public partial class MainWindowViewModel
{
    private SixLabors.ImageSharp.Image<Rgba32>? _source;
    [ObservableProperty] private string? _fileName;
    [ObservableProperty] private IEnumerable<PathGeometry>? _paths;
    [ObservableProperty] private int _width;
    [ObservableProperty] private int _height;
    [ObservableProperty] private int _turdSize = 2;
    [ObservableProperty] private int _turnPolicy = 4;
    [ObservableProperty] private double _alphaMax = 1.0;
    [ObservableProperty] private bool _optiCurve = true;
    [ObservableProperty] private double _optTolerance = 0.2;
    [ObservableProperty] private uint _quantizeUnit = 10;
    [ObservableProperty] private string _filter = "c.R < 128 && c.A > 0";
    [ObservableProperty] private string _fillColor = "#000000";

    public MainWindowViewModel()
    {
        // ReSharper disable AsyncVoidLambda
        this.WhenChanged(x => x.TurdSize).DistinctUntilChanged().Subscribe( async _ => await Trace());
        this.WhenChanged(x => x.TurnPolicy).DistinctUntilChanged().Subscribe(async _ => await Trace());
        this.WhenChanged(x => x.AlphaMax).DistinctUntilChanged().Subscribe(async _ => await Trace());
        this.WhenChanged(x => x.OptiCurve).DistinctUntilChanged().Subscribe(async _ => await Trace());
        this.WhenChanged(x => x.OptTolerance).DistinctUntilChanged().Subscribe(async _ => await Trace());
        this.WhenChanged(x => x.QuantizeUnit).DistinctUntilChanged().Subscribe(async _ => await Trace());
        this.WhenChanged(x => x.Filter).DistinctUntilChanged().Subscribe(async _ => await Trace());
        // ReSharper restore AsyncVoidLambda

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
    
    [RelayCommand]
    private async Task Open()
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
                await OpenStream(stream, file.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    [RelayCommand]
    private async Task Save()
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
                await SaveStream(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    public async Task SaveStream(Stream stream)
    {
        if (_paths is not null)
        {
            await SvgWriter.Save(Width, _height, _paths, stream, FillColor);
        }
    }

    public async Task OpenStream(Stream stream, string filename)
    {
        Decode(stream);

        await Trace();

        FileName = filename;
    }

    private void Decode(Stream stream)
    {
        _source?.Dispose();
        _source = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
    }

    private async Task Trace()
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
