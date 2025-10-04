using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using VTFBatcher.Enums;
using VTFBatcher.ViewModels;

namespace VTFBatcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private static readonly string[] AvailableExtensions = [".bmp", ".dds", ".gif", ".jpeg", ".jpg", ".png", ".tga"];

    /// <summary>
    /// 检查拖拽的是否为文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PicturePathListBox_OnDragOver(object? sender, DragEventArgs e)
    {
        bool allow = false;
        if (e.Data.Contains(DataFormats.Files) || e.Data.Contains(DataFormats.FileNames))
        {
            var items = e.Data.GetFiles();
            if (items != null)
            {
                foreach (var item in items)
                {
                    var path = item.TryGetLocalPath();
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                        if (AvailableExtensions.Contains(ext))
                        {
                            allow = true;
                            break;
                        }
                    }
                }
                // Parallel.ForEach(items, item =>
                // {
                //     var path = item.TryGetLocalPath();
                //     if (!string.IsNullOrWhiteSpace(path))
                //     {
                //         var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                //         if (AvailableExtensions.Contains(ext))
                //         {
                //             allow = true;
                //         }
                //     }
                // });
            }
        }

        e.DragEffects = allow ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void PicturePathListBox_OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            e.Handled = true;
            return;
        }

        var items = e.Data.GetFiles();
        if (items is not null)
        {
            // Stopwatch spw = new();
            // spw.Start();
            foreach (var item in items)
            {
                var path = item.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                    if (AvailableExtensions.Contains(ext) && !vm.PicturePaths.Contains(path))
                    {
                        vm.PicturePaths.Add(path);
                    }
                }
            }

            // ConcurrentBag<string> importedPaths = new();
            //
            // Parallel.ForEach(items, item =>
            // {
            //     var path = item.TryGetLocalPath();
            //     if (!string.IsNullOrWhiteSpace(path))
            //     {
            //         var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            //         if (AvailableExtensions.Contains(ext) && !vm.PicturePaths.Contains(path))
            //         {
            //             importedPaths.Add(path);
            //         }
            //     }
            // });
            // foreach (var path in importedPaths)
            // {
            //     vm.PicturePaths.Add(path);
            // }
            // spw.Stop();
            // Debug.WriteLine($"took {spw.ElapsedMilliseconds} ms");
        }

        e.Handled = true;
    }

    private void PicturePathListBox_DeleteKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            PicturePathListBox_DeleteItem();
        }
    }

    private void PicturePathListBox_DeleteItem()
    {
        if (PicturePathListBox.SelectedItems.Count <= 0 || DataContext is not MainWindowViewModel vm) return;

        var selectedItems = PicturePathListBox.SelectedItems.Cast<string>().ToList();
        vm.DeleteSelectedItemsCommand.Execute(selectedItems);
    }

    private void PicturePathListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (sender is not ListBox listBox) return;
        vm.SelectedPicturePath = new List<string>(listBox.SelectedItems.Cast<string>());
    }
}