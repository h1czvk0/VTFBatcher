using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    // 正常贴图
    [ObservableProperty] private ObservableCollection<string> _normalPicturePaths = new();
    [ObservableProperty] private List<string> _selectedNormalPicturePath = new();

    // 倒地贴图
    [ObservableProperty] private ObservableCollection<string> _incapPicturePaths = new();
    [ObservableProperty] private List<string> _selectedIncapPicturePath = new();

    public Array ImageFormats => Enum.GetValues<ImageFormatEnum>();
    [ObservableProperty] private int _selectedNormalFormatIndex = 0;
    [ObservableProperty] private int _selectedAlphaFormatIndex = 0;

    [ObservableProperty] private bool _ifResize = false;
    [ObservableProperty] private bool _ifClamp = false;
    public bool IfResizeAndClamp => IfClamp && IfResize;

    public Array ResizeMethods => Enum.GetValues<ResizeMethodEnum>()
        .Cast<ResizeMethodEnum>()
        .Select(e => e + "PowerOfTwo").ToArray();

    [ObservableProperty] private int _selectedResizeMethodIndex = 0;
    public Array ResizeFilters => Enum.GetValues<ResizeFilterEnum>();
    [ObservableProperty] private int _selectedResizeFilterIndex = 0;
    public Array SharpenFilters => Enum.GetValues<SharpenFilterEnum>();
    [ObservableProperty] private int _selectedResizeSharpenFilterIndex = 0;

    public List<int> ClampSizes => Enum.GetValues<SizeEnum>().Cast<SizeEnum>().Select(e => (int)e).ToList();
    [ObservableProperty] private int _selectedMaximumWidthIndex = 0;
    [ObservableProperty] private int _selectedMaximumHeightIndex = 0;

    [ObservableProperty] private bool _ifGenerateMipmaps = false;
    [ObservableProperty] private int _selectedMipmapFilterIndex = 0;
    [ObservableProperty] private int _selectedMipmapSharpenFilterIndex = 0;

    [ObservableProperty] private string _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output");

    // ReSharper disable once InconsistentNaming
    [ObservableProperty] private static List<string> _VTFVersions = new() { "7.4", "7.3", "7.2", "7.1", "7.0" };
    [ObservableProperty] private string _version = _VTFVersions[2];

    [ObservableProperty] private bool _presetAvailable = false;
    [ObservableProperty] private PresetEnum _selectedPreset = PresetEnum.None;
    [ObservableProperty] private Dictionary<string, PresetEnum> _presets = new();

    public bool PresetNick
    {
        get => SelectedPreset.HasFlag(PresetEnum.Nick);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Nick : SelectedPreset & ~PresetEnum.Nick; OnPropertyChanged(nameof(PresetNick)); }
    }
    public bool PresetEllis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Ellis);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Ellis : SelectedPreset & ~PresetEnum.Ellis; OnPropertyChanged(nameof(PresetEllis)); }
    }
    public bool PresetRochelle
    {
        get => SelectedPreset.HasFlag(PresetEnum.Rochelle);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Rochelle : SelectedPreset & ~PresetEnum.Rochelle; OnPropertyChanged(nameof(PresetRochelle)); }
    }
    public bool PresetCoach
    {
        get => SelectedPreset.HasFlag(PresetEnum.Coach);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Coach : SelectedPreset & ~PresetEnum.Coach; OnPropertyChanged(nameof(PresetCoach)); }
    }
    public bool PresetBill
    {
        get => SelectedPreset.HasFlag(PresetEnum.Bill);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Bill : SelectedPreset & ~PresetEnum.Bill; OnPropertyChanged(nameof(PresetBill)); }
    }
    public bool PresetLouis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Louis);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Louis : SelectedPreset & ~PresetEnum.Louis; OnPropertyChanged(nameof(PresetLouis)); }
    }
    public bool PresetZoey
    {
        get => SelectedPreset.HasFlag(PresetEnum.Zoey);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Zoey : SelectedPreset & ~PresetEnum.Zoey; OnPropertyChanged(nameof(PresetZoey)); }
    }
    public bool PresetFrancis
    {
        get => SelectedPreset.HasFlag(PresetEnum.Francis);
        set { SelectedPreset = value ? SelectedPreset | PresetEnum.Francis : SelectedPreset & ~PresetEnum.Francis; OnPropertyChanged(nameof(PresetFrancis)); }
    }

    // 集合变化时检查可用性
    partial void OnNormalPicturePathsChanged(ObservableCollection<string> value) => CheckPresetAvailability();
    partial void OnIncapPicturePathsChanged(ObservableCollection<string> value) => CheckPresetAvailability();

    private void CheckPresetAvailability()
    {
        PresetAvailable = NormalPicturePaths.Count > 0 &&
                          IncapPicturePaths.Count > 0 &&
                          NormalPicturePaths.Count == IncapPicturePaths.Count;
    }

    partial void OnSelectedPresetChanged(PresetEnum value) => RefreshCheckBoxProperties();

    private void RefreshCheckBoxProperties()
    {
        OnPropertyChanged(nameof(PresetNick));
        OnPropertyChanged(nameof(PresetEllis));
        OnPropertyChanged(nameof(PresetRochelle));
        OnPropertyChanged(nameof(PresetCoach));
        OnPropertyChanged(nameof(PresetBill));
        OnPropertyChanged(nameof(PresetLouis));
        OnPropertyChanged(nameof(PresetZoey));
        OnPropertyChanged(nameof(PresetFrancis));
    }

    [RelayCommand]
    private async Task StartConvertAsync()
    {
        if (!Directory.Exists(OutputDirectory)) Directory.CreateDirectory(OutputDirectory);

        if (NormalPicturePaths.Count != IncapPicturePaths.Count)
        {
            Debug.WriteLine("错误：正常贴图和倒地贴图数量不匹配！");
            return;
        }

        Stopwatch spw = new();
        spw.Start();

        var errorDic = new Dictionary<string, string>();
        var normalTasks = new List<Task<(string, string)>>();
        var incapTasks = new List<Task<(string, string)>>();

        for (int i = 0; i < NormalPicturePaths.Count; i++)
        {
            var normalPath = NormalPicturePaths[i];
            var incapPath = IncapPicturePaths[i];

            if (!string.IsNullOrWhiteSpace(normalPath))
                normalTasks.Add(VTFConverter.ConvertToVTF(ArrangeArgs(normalPath), normalPath));

            if (!string.IsNullOrWhiteSpace(incapPath))
                incapTasks.Add(VTFConverter.ConvertToVTF(ArrangeArgs(incapPath), incapPath));
        }

        var normalResults = await Task.WhenAll(normalTasks);
        var incapResults = await Task.WhenAll(incapTasks);

        foreach (var (k, v) in normalResults.Concat(incapResults))
        {
            if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v))
                errorDic[k] = v;
        }

        if (SelectedPreset != PresetEnum.None)
        {
            for (int i = 0; i < NormalPicturePaths.Count; i++)
            {
                var normalPath = NormalPicturePaths[i];
                var incapPath = IncapPicturePaths[i];

                var normalVtf = Path.Combine(OutputDirectory, Path.ChangeExtension(Path.GetFileName(normalPath), ".vtf"));
                var incapVtf = Path.Combine(OutputDirectory, Path.ChangeExtension(Path.GetFileName(incapPath), ".vtf"));

                if (File.Exists(normalVtf) && File.Exists(incapVtf))
                {
                    try
                    {
                        foreach (var flag in EnumHelper.GetFlags(SelectedPreset))
                            PresetOpreation.PresetActions[flag](normalVtf, incapVtf);
                    }
                    catch (Exception e)
                    {
                        errorDic[$"{normalVtf}+{incapVtf}"] = $"预设应用失败: \n {e.Message}";
                    }
                }
                else
                {
                    errorDic[$"{normalVtf}+{incapVtf}"] = "转换后的文件未找到。预设应用跳过。";
                }
            }
        }

        spw.Stop();

        var crwvm = new ConvertResultWindowViewModel();
        crwvm.InitInfo($"转换完成，用时 {spw.ElapsedMilliseconds} 毫秒，{errorDic.Count} 个文件转换失败", errorDic);
        var convertResultWindow = new ConvertResultWindow { DataContext = crwvm };
        convertResultWindow.InitInfo();
        convertResultWindow.Show();
    }

    // 选择/删除命令（正常贴图）
    [RelayCommand] private async Task SelectNormalInputDirectoryAsync() => await SelectInputDirectoryAsync(NormalPicturePaths);
    [RelayCommand] private async Task SelectNormalInputFilesAsync() => await SelectInputFilesAsync(NormalPicturePaths, "选择正常贴图文件");
    [RelayCommand] private async Task DeleteNormalSelectedItemsAsync(IList? paths) => await DeleteSelectedItemsAsync(paths, NormalPicturePaths);
    [RelayCommand] private async Task OpenNormalItemsDirectoryAsync(IList? paths) => await OpenItemsDirectoryAsync(paths);

    // 选择/删除命令（倒地贴图）
    [RelayCommand] private async Task SelectIncapInputDirectoryAsync() => await SelectInputDirectoryAsync(IncapPicturePaths);
    [RelayCommand] private async Task SelectIncapInputFilesAsync() => await SelectInputFilesAsync(IncapPicturePaths, "选择倒地贴图文件");
    [RelayCommand] private async Task DeleteIncapSelectedItemsAsync(IList? paths) => await DeleteSelectedItemsAsync(paths, IncapPicturePaths);
    [RelayCommand] private async Task OpenIncapItemsDirectoryAsync(IList? paths) => await OpenItemsDirectoryAsync(paths);

    // 通用 I/O
    private async Task SelectInputDirectoryAsync(ObservableCollection<string> target)
    {
        var dir = await SelectDirectoryAsync();
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            var allowed = FilePickerFileTypes.ImageAll.Patterns.Select(x => x.TrimStart('*')).ToList();
            foreach (var f in Directory.GetFiles(dir))
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                if (allowed.Contains(ext) && !target.Contains(f)) target.Add(f);
            }
        }
        CheckPresetAvailability();
    }

    private async Task SelectInputFilesAsync(ObservableCollection<string> target, string title)
    {
        var files = await SelectFileAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });
        foreach (var f in files) if (!target.Contains(f)) target.Add(f);
        CheckPresetAvailability();
    }

    private async Task DeleteSelectedItemsAsync(IList? paths, ObservableCollection<string> target)
    {
        if (paths is null || paths.Count == 0) return;
        foreach (var p in paths.Cast<string>().ToList()) target.Remove(p);
        CheckPresetAvailability();
    }

    [RelayCommand]
    private async Task SelectOutputDirectoryAsync()
    {
        var dir = await SelectDirectoryAsync();
        if (!string.IsNullOrWhiteSpace(dir)) OutputDirectory = dir;
    }

    [RelayCommand] private async Task OpenOutputDirectoryAsync() => await OpenDirectoryAsync(OutputDirectory);

    [RelayCommand]
    private async Task OpenItemsDirectoryAsync(IList? paths)
    {
        if (paths is null || paths.Count == 0) return;
        foreach (var p in paths.Cast<string>())
        {
            var d = Path.GetDirectoryName(p);
            if (!string.IsNullOrEmpty(d)) await OpenDirectoryAsync(d);
        }
    }

    [RelayCommand]
    private async Task ClearAllItemsAsync()
    {
        NormalPicturePaths.Clear();
        IncapPicturePaths.Clear();
        await ResetPresetsAsync();
    }

    [RelayCommand]
    private async Task ResetPresetsAsync()
    {
        Presets.Clear();
        SelectedPreset = PresetEnum.None;
    }

    private async Task<string> SelectDirectoryAsync()
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        if (window == null) return string.Empty;
        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "选择文件夹", AllowMultiple = false });
        var folder = folders?.FirstOrDefault();
        if (folder == null) return string.Empty;
        var path = folder.Path;
        return path.IsAbsoluteUri ? path.LocalPath : path.ToString();
    }

    private async Task<List<string>> SelectFileAsync(FilePickerOpenOptions? opt = null)
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        if (window == null) return new List<string>();
        var res = await window.StorageProvider.OpenFilePickerAsync(opt ?? new FilePickerOpenOptions { Title = "选择文件", AllowMultiple = true, FileTypeFilter = new[] { FilePickerFileTypes.All } });
        return res.Select(p => p.TryGetLocalPath()).Where(p => !string.IsNullOrEmpty(p)).ToList()!;
    }

    private async Task OpenDirectoryAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var psi = new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{path}\"", UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception e) { Debug.WriteLine(e); }
    }

    private string ArrangeArgs(string inputFilePath)
    {
        var sb = new StringBuilder();
        if (IfResize)
        {
            sb.Append($" -resize -rmethod {((ResizeMethodEnum)SelectedResizeMethodIndex)} -rfilter {((ResizeFilterEnum)SelectedResizeFilterIndex)} -rsharpen {((SharpenFilterEnum)SelectedResizeSharpenFilterIndex)}");
            if (IfClamp) sb.Append($" -rclampwidth {ClampSizes[SelectedMaximumWidthIndex]} -rclampheight {ClampSizes[SelectedMaximumHeightIndex]}");
        }
        if (IfGenerateMipmaps) sb.Append($" -mfilter {((ResizeFilterEnum)SelectedMipmapFilterIndex)} -msharpen {((SharpenFilterEnum)SelectedMipmapSharpenFilterIndex)}");
        else sb.Append(" -nomipmaps");

        sb.Append($" -format {((ImageFormatEnum)SelectedNormalFormatIndex)} -alphaformat {((ImageFormatEnum)SelectedAlphaFormatIndex)}");
        sb.Append($" -version {Version}");
        sb.Append($" -file \"{inputFilePath}\" -output \"{OutputDirectory.TrimEnd('\\')}\"");
        return sb.ToString();
    }
}
