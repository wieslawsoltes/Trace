using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp.PixelFormats;

namespace TraceGui.ViewModels;

[ObservableObject]
public partial class SourceImageViewModel : IDisposable
{
    [ObservableProperty] private Avalonia.Media.Imaging.Bitmap? _bitmap;

    public SourceImageViewModel()
    {
    }

    public SourceImageViewModel(SixLabors.ImageSharp.Image<Rgba32> source)
    {
        using var ms = new MemoryStream();
        source.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        ms.Position = 0;
        Bitmap = new Avalonia.Media.Imaging.Bitmap(ms);
    }

    public void Dispose()
    {
        _bitmap?.Dispose();
    }
}
