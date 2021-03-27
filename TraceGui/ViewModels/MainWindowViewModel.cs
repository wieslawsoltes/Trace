using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using BitmapToVector;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ReactiveUI;
using SixLabors.ImageSharp.PixelFormats;

namespace TraceGui.ViewModels
{
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

        private async Task OnOpen()
        {
            var dlg = new OpenFileDialog();

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Supported Files (*.png;*.jpg;*.jpeg;*.bmp)", 
                Extensions = new List<string> {"png", "jpg", "jpeg", "bmp"}
            });

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "All Files", 
                Extensions = new List<string> {"*"}
            });

            var result = await dlg.ShowAsync(GetMainWindow());
            if (result is not null && result.Length == 1)
            {
                Load(result[0]);
            }
        }

        private async Task OnSave()
        {
            var dlg = new SaveFileDialog();

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Svg Files (*.svg)", 
                Extensions = new List<string> {"svg"}
            });

            dlg.InitialFileName = Path.GetFileNameWithoutExtension(_fileName);

            var result = await dlg.ShowAsync(GetMainWindow());
            if (result is not null)
            {
                Save(result);
            }
        }

        private void Save(string filename)
        {
            if (_paths is not null)
            {
                SvgWriter.Save(Width, _height, _paths, filename, FillColor);
            }
        }

        public void Load(string filename)
        {
            Decode(filename);

            Trace();

            FileName = filename;
        }

        private void Decode(string filename)
        {
            _source?.Dispose();
            _source = SixLabors.ImageSharp.Image.Load<Rgba32>(filename);
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
            Func<Rgba32, bool> filter = DefaultFilter;

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

        private Window? GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}