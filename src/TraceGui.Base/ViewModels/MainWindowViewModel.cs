﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TraceGui.ViewModels;

[ObservableObject]
public partial class MainWindowViewModel
{
    [ObservableProperty] private OptionsViewModel _options;
    [ObservableProperty] private TraceResultViewModel? _traceResult;

    public MainWindowViewModel()
    {
        _options = new OptionsViewModel(Trace);

        Task.Run(async () =>
        {
            try
            {
                await TraceResultViewModel.Compile(_options.Filter);
            }
            catch
            {
                Console.WriteLine("Failed to compile user filter.");
            }
        });
    }

    private List<FilePickerFileType> GetOpenFileTypes()
    {
        return new List<FilePickerFileType> { StorageService.ImageAll, StorageService.All };
    }

    private static List<FilePickerFileType> GetSaveFileTypes()
    {
        return new List<FilePickerFileType> { StorageService.ImageSvg, StorageService.All };
    }

    private async Task Trace()
    {
        if (_traceResult is not null)
        {
            await _traceResult.Trace(_options);
        }
    }

    public async Task Load(Stream stream, string fileName)
    {
        var source = await TraceResultViewModel.OpenStream(stream);
        var traceResult = new TraceResultViewModel(source, fileName);
        await traceResult.Trace(_options);

        TraceResult?.Dispose();
        TraceResult = null;
        TraceResult = traceResult;
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
            Title = "Open image", FileTypeFilter = GetOpenFileTypes(), AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null && file.CanOpenRead)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                await Load(stream, file.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_traceResult is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save drawing",
            FileTypeChoices = GetSaveFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension(_traceResult.FileName),
            DefaultExtension = "svg",
            ShowOverwritePrompt = true
        });

        if (file is not null && file.CanOpenWrite)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                await _traceResult.SaveStream(stream, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
