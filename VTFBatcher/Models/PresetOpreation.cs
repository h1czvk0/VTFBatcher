using System;
using System.Collections.Generic;
using System.IO;
using VTFBatcher.Enums;

namespace VTFBatcher.Models;

public static class PresetOpreation
{
    public static Dictionary<PresetEnum, Action<string, string>> PresetActions = new()
    {
        { PresetEnum.Nick, ApplyNickPreset },
        { PresetEnum.Ellis, ApplyEllisPreset },
        { PresetEnum.Rochelle, ApplyRochellePreset },
        { PresetEnum.Coach, ApplyCoachPreset },
        { PresetEnum.Bill, ApplyBillPreset },
        { PresetEnum.Louis, ApplyLouisPreset },
        { PresetEnum.Zoey, ApplyZoeyPreset },
        { PresetEnum.Francis, ApplyFrancisPreset },
    };

    // 角色名称映射
    private static Dictionary<PresetEnum, string> CharacterFolderNames = new()
    {
        { PresetEnum.Nick, "Nick" },
        { PresetEnum.Ellis, "Ellis" },
        { PresetEnum.Rochelle, "Rochelle" },
        { PresetEnum.Coach, "Coach" },
        { PresetEnum.Bill, "Bill" },
        { PresetEnum.Louis, "Louis" },
        { PresetEnum.Zoey, "Zoey" },
        { PresetEnum.Francis, "Francis" },
    };

    private static void ApplyNickPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_gambler", "s_panel_gambler_incap", "s_panel_lobby_gambler", PresetEnum.Nick);
    }

    private static void ApplyEllisPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_mechanic", "s_panel_mechanic_incap", "s_panel_lobby_mechanic", PresetEnum.Ellis);
    }

    private static void ApplyRochellePreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_producer", "s_panel_producer_incap", "s_panel_lobby_producer", PresetEnum.Rochelle);
    }

    private static void ApplyCoachPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_coach", "s_panel_coach_incap", "s_panel_lobby_coach", PresetEnum.Coach);
    }

    private static void ApplyBillPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_namvet", "s_panel_namvet_incap", "select_bill", PresetEnum.Bill);
    }

    private static void ApplyLouisPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_manager", "s_panel_manager_incap", "select_louis", PresetEnum.Louis);
    }

    private static void ApplyZoeyPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_teenangst", "s_panel_teenangst_incap", "select_zoey", PresetEnum.Zoey);
    }

    private static void ApplyFrancisPreset(string normalVtfPath, string incapVtfPath)
    {
        CreateCharacterVGUI(normalVtfPath, incapVtfPath, "s_panel_biker", "s_panel_biker_incap", "select_francis", PresetEnum.Francis);
    }

    // ReSharper disable once InconsistentNaming
    private static void CreateCharacterVGUI(string normalVtfPath, string incapVtfPath, string vgui1, string vgui2, string vgui3, PresetEnum character)
    {
        if (!File.Exists(normalVtfPath)) throw new FileNotFoundException("正常贴图VTF文件未找到。", normalVtfPath);
        if (!File.Exists(incapVtfPath)) throw new FileNotFoundException("倒地贴图VTF文件未找到。", incapVtfPath);

        var baseDir = Path.GetDirectoryName(normalVtfPath);
        if (baseDir == null) throw new DirectoryNotFoundException("VTF文件目录未找到。");

        // 创建角色专用文件夹
        var characterFolderName = CharacterFolderNames[character];
        var characterDir = Path.Combine(baseDir, characterFolderName);

        // 如果文件夹不存在，创建它
        if (!Directory.Exists(characterDir))
        {
            Directory.CreateDirectory(characterDir);
        }

        // 使用正常贴图创建两个文件（正常状态和选择界面）
        File.Copy(normalVtfPath, Path.Combine(characterDir, vgui1 + ".vtf"), true);
        File.Copy(normalVtfPath, Path.Combine(characterDir, vgui3 + ".vtf"), true);

        // 使用倒地贴图创建incap文件
        File.Copy(incapVtfPath, Path.Combine(characterDir, vgui2 + ".vtf"), true);
    }
}
