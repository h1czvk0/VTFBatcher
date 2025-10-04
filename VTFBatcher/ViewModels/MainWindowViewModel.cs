using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VTFBatcher.Models;
using VTFBatcher.Enums;
using VTFBatcher.Utils;
using VTFBatcher.Views;

namespace VTFBatcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<string> _picturePaths = new();
    [ObservableProperty] private List<string> _selectedPicturePath = new();

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

    // ReSharper disable once InconsistentNaming
    [ObservableProperty] private static List<string> _VTFVersions = new() { "7.4", "7.3", "7.2", "7.1", "7.0" };

    [ObservableProperty] private string _version = _VTFVersions[0];

    [ObservableProperty] private bool _presetAvailable = false;
    [ObservableProperty] private PresetEnum _selectedPreset = PresetEnum.None;
    [ObservableProperty] private Dictionary<string, PresetEnum> _presets = new();

    public bool PresetNick
    {
        get => SelectedPreset.HasFlag(PresetEnum.Nick);
        set
        {
            SelectedPreset = value
                ? SelectedPreset | PresetEnum.Nick
                : SelectedPreset & ~PresetEnum.Nick;
            OnPropertyChanged(nameof(PresetNick));
        }
    }

    public bool PresetEllis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Ellis);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Ellis : SelectedPreset & ~PresetEnum.Ellis;
            OnPropertyChanged(nameof(PresetEllis));
        }
    }

    public bool PresetRochelle
    {
        get => SelectedPreset.HasFlag(PresetEnum.Rochelle);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Rochelle : SelectedPreset & ~PresetEnum.Rochelle;
            OnPropertyChanged(nameof(PresetRochelle));
        }
    }

    public bool PresetCoach
    {
        get => SelectedPreset.HasFlag(PresetEnum.Coach);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Coach : SelectedPreset & ~PresetEnum.Coach;
            OnPropertyChanged(nameof(PresetCoach));
        }
    }

    public bool PresetBill
    {
        get => SelectedPreset.HasFlag(PresetEnum.Bill);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Bill : SelectedPreset & ~PresetEnum.Bill;
            OnPropertyChanged(nameof(PresetBill));
        }
    }

    public bool PresetLouis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Louis);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Louis : SelectedPreset & ~PresetEnum.Louis;
            OnPropertyChanged(nameof(PresetLouis));
        }
    }

    public bool PresetZoey
    {
        get => SelectedPreset.HasFlag(PresetEnum.Zoey);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Zoey : SelectedPreset & ~PresetEnum.Zoey;
            OnPropertyChanged(nameof(PresetZoey));
        }
    }

    public bool PresetFrancis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Francis);
        set
        {
            SelectedPreset = value ? SelectedPreset | PresetEnum.Francis : SelectedPreset & ~PresetEnum.Francis;
            OnPropertyChanged(nameof(PresetFrancis));
        }
    }

    partial void OnSelectedPicturePathChanged(List<string> value)
    {
        if (value.Count != 1)
        {
            PresetAvailable = false;
            return;
        }

        PresetAvailable = true;
        if (Presets.ContainsKey(value[0]))
        {
            SelectedPreset = Presets[value[0]];
        }
        else
        {
            SelectedPreset = PresetEnum.None;
        }
    }

    partial void OnSelectedPresetChanged(PresetEnum value)
    {
        if (SelectedPicturePath.Count != 1) return;
        var path = SelectedPicturePath[0];
        if (Presets.ContainsKey(path))
        {
            Presets[path] = value;
        }
        else
        {
            Presets.Add(path, value);
        }

        RefreshCheckBoxProperties();
    }

    private void RefreshCheckBoxProperties()
    {
        PresetNick = SelectedPreset.HasFlag(PresetEnum.Nick);
        PresetEllis = SelectedPreset.HasFlag(PresetEnum.Ellis);
        PresetRochelle = SelectedPreset.HasFlag(PresetEnum.Rochelle);
        PresetCoach = SelectedPreset.HasFlag(PresetEnum.Coach);
        PresetBill = SelectedPreset.HasFlag(PresetEnum.Bill);
        PresetLouis = SelectedPreset.HasFlag(PresetEnum.Louis);
        PresetZoey = SelectedPreset.HasFlag(PresetEnum.Zoey);
        PresetFrancis = SelectedPreset.HasFlag(PresetEnum.Francis);
    }

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

        Debug.WriteLine("Finished all conversions.");

        foreach (var result in results)
        {
            if (!string.IsNullOrEmpty(result.Item1) && !string.IsNullOrEmpty(result.Item2))
            {
                errorDic.Add(result.Item1, result.Item2);
            }
        }

        foreach (var path in PicturePaths)
        {
            if (!String.IsNullOrEmpty(path) && !errorDic.ContainsKey(path) &&
                Presets.TryGetValue(path, out PresetEnum preset) && preset != PresetEnum.None)
            {
                var outputFile = Path.Combine(OutputDirectory, Path.ChangeExtension(Path.GetFileName(path), ".vtf"));
                if (File.Exists(outputFile))
                {
                    try
                    {
                        foreach (var flag in EnumHelper.GetFlags(preset))
                        {
                            PresetOpreation.PresetActions[flag](outputFile);
                        }
                    }
                    catch (Exception e)
                    {
                        errorDic.Add(outputFile, $"Preset application failed: \n {e.Message}");
                    }
                }
                else
                {
                    errorDic.Add(outputFile, $"Converted file \"{outputFile}\" not found. Preset application skipped.");
                }
            }
        }

        spw.Stop();

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

    [RelayCommand]
    private async Task SelectInputDirectoryAsync()
    {
        var dir = await SelectDirectoryAsync();
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            var AvailableExtensions = FilePickerFileTypes.ImageAll.Patterns.Select(x => x.TrimStart('*')).ToList();
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file).ToLowerInvariant();
                if (AvailableExtensions.Contains(ext) && !PicturePaths.Contains(file))
                {
                    PicturePaths.Add(file);
                }
            }
        }
    }

    [RelayCommand]
    private async Task SelectInputFilesAsync()
    {
        var files = await SelectFileAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "选择文件",
            AllowMultiple = true,
            FileTypeFilter =
            [
                FilePickerFileTypes.ImageAll
            ]
        });
        foreach (var file in files)
        {
            if (!PicturePaths.Contains(file))
            {
                PicturePaths.Add(file);
            }
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
            AllowMultiple = false,
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

    private async Task<List<string>> SelectFileAsync(FilePickerOpenOptions? filePickerOpenOptions = null)
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (window == null) return new List<string>();
        var storageProvider = window.StorageProvider;

        var path = await storageProvider.OpenFilePickerAsync(filePickerOpenOptions ??
                                                             new Avalonia.Platform.Storage.FilePickerOpenOptions
                                                             {
                                                                 Title = "选择文件",
                                                                 AllowMultiple = true,
                                                                 FileTypeFilter = new[] { FilePickerFileTypes.All }
                                                             });

        return path.Select(p => p.TryGetLocalPath()).ToList();
    }

    [RelayCommand]
    private async Task OpenOutputDirectoryAsync()
    {
        await OpenDirectoryAsync(OutputDirectory);
    }

    [RelayCommand]
    private async Task OpenItemsDirectoryAsync(IList? paths)
    {
        if (paths is null || paths.Count == 0) return;

        foreach (var path in paths.Cast<string>())
        {
            OpenDirectoryAsync(Path.GetDirectoryName(path));
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedItemsAsync(IList? paths)
    {
        if (paths is null || paths.Count == 0) return;

        var toRemove = paths.Cast<string>().ToList();
        foreach (var path in toRemove)
        {
            PicturePaths.Remove(path);
            if (Presets.ContainsKey(path))
            {
                Presets.Remove(path);
            }
        }
    }

    [RelayCommand]
    private async Task ClearAllItemsAsync()
    {
        PicturePaths.Clear();
        await ResetPresetsAsync();
    }

    [RelayCommand]
    private async Task ResetPresetsAsync()
    {
        Presets.Clear();
        SelectedPreset = PresetEnum.None;
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
            $" -file \"{inputFilePath}\" -output \"{OutputDirectory.TrimEnd('\\')}\"");
        Debug.WriteLine(sb.ToString());
        return sb.ToString();
    }
}