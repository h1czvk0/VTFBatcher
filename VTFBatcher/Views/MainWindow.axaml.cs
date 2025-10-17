using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using VTFBatcher.ViewModels;

namespace VTFBatcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NormalPicturePathListBox_DeleteKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && sender is ListBox listBox && DataContext is MainWindowViewModel vm)
        {
            vm.DeleteNormalSelectedItemsCommand.Execute(listBox.SelectedItems);
        }
    }

    private void NormalPicturePathListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && DataContext is MainWindowViewModel vm)
        {
            vm.SelectedNormalPicturePath = listBox.SelectedItems?.Cast<string>().ToList() ?? new List<string>();
        }
    }

    private void NormalPicturePathListBox_OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private void NormalPicturePathListBox_OnDrop(object? sender, DragEventArgs e)
    {
        HandleFileDrop(e, true);
    }

    private void IncapPicturePathListBox_DeleteKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && sender is ListBox listBox && DataContext is MainWindowViewModel vm)
        {
            vm.DeleteIncapSelectedItemsCommand.Execute(listBox.SelectedItems);
        }
    }

    private void IncapPicturePathListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && DataContext is MainWindowViewModel vm)
        {
            vm.SelectedIncapPicturePath = listBox.SelectedItems?.Cast<string>().ToList() ?? new List<string>();
        }
    }

    private void IncapPicturePathListBox_OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private void IncapPicturePathListBox_OnDrop(object? sender, DragEventArgs e)
    {
        HandleFileDrop(e, false);
    }

    private void HandleFileDrop(DragEventArgs e, bool isNormalTexture)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var files = e.Data.GetFiles();
        if (files == null) return;

        var availableExtensions = FilePickerFileTypes.ImageAll.Patterns.Select(x => x.TrimStart('*')).ToList();
        var targetCollection = isNormalTexture ? vm.NormalPicturePaths : vm.IncapPicturePaths;

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (availableExtensions.Contains(ext) && !targetCollection.Contains(path))
                {
                    targetCollection.Add(path);
                }
            }
        }
    }
}
