# Trace

[![Build status](https://dev.azure.com/wieslawsoltes/GitHub/_apis/build/status/Trace)](https://dev.azure.com/wieslawsoltes/GitHub/_build/latest?definitionId=84)

[![GitHub release](https://img.shields.io/github/release/wieslawsoltes/trace.svg)](https://github.com/wieslawsoltes/trace)
[![Github All Releases](https://img.shields.io/github/downloads/wieslawsoltes/trace/total.svg)](https://github.com/wieslawsoltes/trace)
[![Github Releases](https://img.shields.io/github/downloads/wieslawsoltes/trace/latest/total.svg)](https://github.com/wieslawsoltes/trace)

Trace is an application for transforming bitmaps into vector graphics using [BitmapToVector](https://github.com/daltonks/BitmapToVector) library.

![](images/TraceGui.png)

* [BitmapToVector](https://github.com/daltonks/BitmapToVector) C# port of Potrace with optional SkiaSharp support.
* [Potrace](http://potrace.sourceforge.net/) Transforming bitmaps into vector graphics.
* [mkbitmap](http://potrace.sourceforge.net/mkbitmap.html) Transform images into bitmaps with scaling and filtering.

# Building

Download and install [.NET 5.0 SDK](https://dotnet.microsoft.com/download).

### Build

```bash
dotnet build ./src/TraceGui/TraceGui.csproj -c Release
```

### Run

```bash
dotnet run --project ./src/TraceGui/TraceGui.csproj -c Release
```

### Publish

```bash
dotnet publish ./src/TraceGui/TraceGui.csproj -c Release -f net5.0 -r win7-x64 -o TraceGui-win7-x64
```

```bash
dotnet publish ./src/TraceGui/TraceGui.csproj -c Release -f net5.0 -r debian.8-x64 -o TraceGui-debian.8-x64
```

```bash
dotnet publish ./src/TraceGui/TraceGui.csproj -c Release -f net5.0 -r ubuntu.14.04-x64 -o TraceGui-ubuntu.14.04-x64
```

```bash
dotnet publish ./src/TraceGui/TraceGui.csproj -c Release -f net5.0 -r osx.10.12-x64 -o TraceGui-osx.10.12-x64
```

# Licensing

Trace is licensed under the [GPL-3.0 License](LICENSE).
