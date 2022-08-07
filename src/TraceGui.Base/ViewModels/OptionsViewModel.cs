using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveMarbles.PropertyChanged;

namespace TraceGui.ViewModels;

[ObservableObject]
public partial class OptionsViewModel
{
    [ObservableProperty] private int _turdSize = 2;
    [ObservableProperty] private int _turnPolicy = 4;
    [ObservableProperty] private double _alphaMax = 1.0;
    [ObservableProperty] private bool _optiCurve = true;
    [ObservableProperty] private double _optTolerance = 0.2;
    [ObservableProperty] private uint _quantizeUnit = 10;
    [ObservableProperty] private string _filter = "c.R < 128 && c.A > 0";
    [ObservableProperty] private string _fillColor = "#000000";

    public OptionsViewModel()
    {
    }

    public OptionsViewModel(Func<Task> trace)
    {
        // ReSharper disable AsyncVoidLambda
        this.WhenChanged(x => x.TurdSize).DistinctUntilChanged().Subscribe( async _ => await trace());
        this.WhenChanged(x => x.TurnPolicy).DistinctUntilChanged().Subscribe(async _ => await trace());
        this.WhenChanged(x => x.AlphaMax).DistinctUntilChanged().Subscribe(async _ => await trace());
        this.WhenChanged(x => x.OptiCurve).DistinctUntilChanged().Subscribe(async _ => await trace());
        this.WhenChanged(x => x.OptTolerance).DistinctUntilChanged().Subscribe(async _ => await trace());
        this.WhenChanged(x => x.QuantizeUnit).DistinctUntilChanged().Subscribe(async _ => await trace());
        this.WhenChanged(x => x.Filter).DistinctUntilChanged().Subscribe(async _ => await trace());
        // ReSharper restore AsyncVoidLambda
    }
}
