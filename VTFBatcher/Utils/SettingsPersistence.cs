using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using VTFBatcher.ViewModels;

namespace VTFBatcher.Utils;

public static class SettingsPersistence
{
    private const string FileName = "settings.json";

    private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, FileName);

    private class SettingsDto
    {
        public int SelectedNormalFormatIndex { get; set; }
        public int SelectedAlphaFormatIndex { get; set; }
        public bool IfResize { get; set; }
        public bool IfClamp { get; set; }
        public int SelectedResizeMethodIndex { get; set; }
        public int SelectedResizeFilterIndex { get; set; }
        public int SelectedResizeSharpenFilterIndex { get; set; }
        public int SelectedMaximumWidthIndex { get; set; }
        public int SelectedMaximumHeightIndex { get; set; }
        public bool IfGenerateMipmaps { get; set; }
        public int SelectedMipmapFilterIndex { get; set; }
        public int SelectedMipmapSharpenFilterIndex { get; set; }
        public string? Version { get; set; }
        public string? OutputDirectory { get; set; }
    }

    public static void Save(MainWindowViewModel vm)
    {
        try
        {
            var dto = new SettingsDto
            {
                SelectedNormalFormatIndex = vm.SelectedNormalFormatIndex,
                SelectedAlphaFormatIndex = vm.SelectedAlphaFormatIndex,
                IfResize = vm.IfResize,
                IfClamp = vm.IfClamp,
                SelectedResizeMethodIndex = vm.SelectedResizeMethodIndex,
                SelectedResizeFilterIndex = vm.SelectedResizeFilterIndex,
                SelectedResizeSharpenFilterIndex = vm.SelectedResizeSharpenFilterIndex,
                SelectedMaximumWidthIndex = vm.SelectedMaximumWidthIndex,
                SelectedMaximumHeightIndex = vm.SelectedMaximumHeightIndex,
                IfGenerateMipmaps = vm.IfGenerateMipmaps,
                SelectedMipmapFilterIndex = vm.SelectedMipmapFilterIndex,
                SelectedMipmapSharpenFilterIndex = vm.SelectedMipmapSharpenFilterIndex,
                Version = vm.Version,
                OutputDirectory = vm.OutputDirectory,
            };
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 忽略保存异常
        }
    }

    public static void Load(MainWindowViewModel vm)
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;
            var json = File.ReadAllText(SettingsPath);
            var dto = JsonSerializer.Deserialize<SettingsDto>(json);
            if (dto == null) return;

            vm.SelectedNormalFormatIndex = dto.SelectedNormalFormatIndex;
            vm.SelectedAlphaFormatIndex = dto.SelectedAlphaFormatIndex;
            vm.IfResize = dto.IfResize;
            vm.IfClamp = dto.IfClamp;
            vm.SelectedResizeMethodIndex = dto.SelectedResizeMethodIndex;
            vm.SelectedResizeFilterIndex = dto.SelectedResizeFilterIndex;
            vm.SelectedResizeSharpenFilterIndex = dto.SelectedResizeSharpenFilterIndex;
            vm.SelectedMaximumWidthIndex = dto.SelectedMaximumWidthIndex;
            vm.SelectedMaximumHeightIndex = dto.SelectedMaximumHeightIndex;
            vm.IfGenerateMipmaps = dto.IfGenerateMipmaps;
            vm.SelectedMipmapFilterIndex = dto.SelectedMipmapFilterIndex;
            vm.SelectedMipmapSharpenFilterIndex = dto.SelectedMipmapSharpenFilterIndex;
            if (!string.IsNullOrWhiteSpace(dto.Version)) vm.Version = dto.Version;
            if (!string.IsNullOrWhiteSpace(dto.OutputDirectory)) vm.OutputDirectory = dto.OutputDirectory;
        }
        catch
        {
            // 忽略加载异常
        }
    }
}
