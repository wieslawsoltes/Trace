using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TraceGui.ViewModels;

namespace TraceGui.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var dockPanel = this.FindControl<DockPanel>("DockPanel");
            dockPanel.AddHandler(DragDrop.DragOverEvent, DragOver);
            dockPanel.AddHandler(DragDrop.DropEvent, Drop);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            if (!e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var fileName = e.Data.GetFileNames()?.FirstOrDefault();
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (DataContext is MainWindowViewModel vm)
                    {
                        vm.Load(fileName);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}