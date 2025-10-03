using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VTFBatcher.Models;
using VTFBatcher.Enums;
using VTFBatcher.Views;

namespace VTFBatcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<string> _picturePaths = new();
    public Array ImageFormats => Enum.GetValues(typeof(ImageFormatEnum));
    [ObservableProperty] private int _selectedNormalFormatIndex = 0;
    [ObservableProperty] private int _selectedAlphaFormatIndex = 0;

    [ObservableProperty] private bool _ifResize = false;
    [ObservableProperty] private bool _ifClamp = false;
    public bool IfResizeAndClamp => IfClamp && IfResize;

    public Array ResizeMethods => Enum.GetValues(typeof(ResizeMethodEnum)).Cast<ResizeMethodEnum>()
        .Select(e => e + "PowerOfTwo").ToArray();

    [ObservableProperty] private int _selectedResizeMethodIndex = 0;
    public Array ResizeFilters => Enum.GetValues(typeof(ResizeFilterEnum));
    [ObservableProperty] private int _selectedResizeFilterIndex = 0;
    public Array SharpenFilters => Enum.GetValues(typeof(SharpenFilterEnum));
    [ObservableProperty] private int _selectedResizeSharpenFilterIndex = 0;

    public List<int> ClampSizes => Enum.GetValues(typeof(SizeEnum)).Cast<SizeEnum>().Select(e => (int)e).ToList();
    [ObservableProperty] private int _selectedMaximumWidthIndex = 0;
    [ObservableProperty] private int _selectedMaximumHeightIndex = 0;

    [ObservableProperty] private bool _ifGenerateMipmaps = false;
    [ObservableProperty] private int _selectedMipmapFilterIndex = 0;
    [ObservableProperty] private int _selectedMipmapSharpenFilterIndex = 0;

    [ObservableProperty] private string _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output");

    [ObservableProperty] public static List<string> _VTFVersions = new() { "7.4", "7.3", "7.2", "7.1", "7.0" };

    [ObservableProperty] private string _version = _VTFVersions[0];

    [RelayCommand]
    private async Task StartConvertAsync()
    {
        if (!Directory.Exists(OutputDirectory))
            Directory.CreateDirectory(OutputDirectory);
        Stopwatch spw = new();
        spw.Start();

        var tasks = new List<Task<(string, string)>>();
        var errorDic = new Dictionary<string, string>();
        foreach (var item in PicturePaths)
        {
            if (String.IsNullOrEmpty(item)) continue;
            var task = VTFConverter.ConvertToVTF(ArrangeArgs(item), item);
            tasks.Add(task);
            // VTFConverter.ConvertToVTF(ArrangeArgs(item));
        }

        var results = await Task.WhenAll(tasks);
        spw.Stop();
        Debug.WriteLine("Finished all conversions.");

        foreach (var error in results)
        {
            if (!string.IsNullOrEmpty(error.Item1) && !string.IsNullOrEmpty(error.Item2))
                errorDic.Add(error.Item1, error.Item2);
        }

        var crwvm = new ConvertResultWindowViewModel();
        crwvm.InitInfo(
            $"Convert complete in {spw.ElapsedMilliseconds} ms, {errorDic.Count} image convert failed",
            errorDic);
        var convertResultWindow = new ConvertResultWindow
        {
            DataContext = crwvm,
        };
        convertResultWindow.InitInfo();
        convertResultWindow.Show();
    }

    [RelayCommand]
    private async Task SelectOutputDirectoryAsync()
    {
        var dir = await SelectDirectoryAsync();
        if (!string.IsNullOrWhiteSpace(dir))
        {
            OutputDirectory = dir;
        }
    }

    private async Task<string> SelectDirectoryAsync()
    {
        var window =
            Avalonia.Application.Current?.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (window == null) return string.Empty;
        var storageProvider = window.StorageProvider;
        var folders = await storageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "选择文件夹",
            AllowMultiple = false
        });
        var folder = folders?.FirstOrDefault();
        if (folder == null) return string.Empty;
        var path = folder.Path;
        if (path.IsAbsoluteUri)
            return path.LocalPath;
        // ReSharper disable once RedundantIfElseBlock
        else
            return path.ToString();
    }

    [RelayCommand]
    private async Task OpenOutputDirectoryAsync()
    {
        await OpenDirectoryAsync(OutputDirectory);
    }

    private async Task OpenDirectoryAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private string ArrangeArgs(string inputFilePath)
    {
        var sb = new StringBuilder();

        if (IfResize)
        {
            sb.Append(
                $" -resize -rmethod {((ResizeMethodEnum)SelectedResizeMethodIndex).ToString()} -rfilter {((ResizeFilterEnum)SelectedResizeFilterIndex).ToString()} -rsharpen {((SharpenFilterEnum)SelectedResizeSharpenFilterIndex).ToString()}");
            // sb.Append( $" -rwidth {ClampSizes[SelectedMaximumWidthIndex]} -rheight {ClampSizes[SelectedMaximumHeightIndex]}");
            if (IfClamp)
            {
                sb.Append(
                    $" -rclampwidth {ClampSizes[SelectedMaximumWidthIndex]} -rclampheight {ClampSizes[SelectedMaximumHeightIndex]}");
            }
        }

        if (IfGenerateMipmaps)
        {
            sb.Append(
                $" -mfilter {((ResizeFilterEnum)SelectedMipmapFilterIndex).ToString()} -msharpen {((SharpenFilterEnum)SelectedMipmapSharpenFilterIndex).ToString()}");
        }
        else
        {
            sb.Append($" -nomipmaps");
        }

        sb.Append(
            $" -format {((ImageFormatEnum)SelectedNormalFormatIndex).ToString()} -alphaformat {((ImageFormatEnum)SelectedAlphaFormatIndex).ToString()}");
        sb.Append($" -version {Version}");
        sb.Append(
            $" -file \"{inputFilePath}\" -output \"{OutputDirectory}\"");
        Debug.WriteLine(sb.ToString());
        return sb.ToString();
    }
}