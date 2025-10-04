using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using VTFBatcher.Enums;

namespace VTFBatcher.Models;

public static class PresetOpreation
{
    public static Dictionary<PresetEnum, Action<string>> PresetActions = new()
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

    private static void ApplyNickPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_gambler", "s_panel_gambler_incap", "s_panel_lobby_gambler");
    }

    private static void ApplyEllisPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_mechanic", "s_panel_mechanic_incap", "s_panel_lobby_mechanic");
    }

    private static void ApplyRochellePreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_producer", "s_panel_producer_incap", "s_panel_lobby_producer");
    }

    private static void ApplyCoachPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_coach", "s_panel_coach_incap", "s_panel_lobby_coach");
    }

    private static void ApplyBillPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_namvet", "s_panel_namvet_incap", "select_bill");
    }

    private static void ApplyLouisPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_manager", "s_panel_manager_incap", "select_louis");
    }

    private static void ApplyZoeyPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_teenangst", "s_panel_teenangst_incap", "select_zoey");
    }

    private static void ApplyFrancisPreset(string filePath)
    {
        CreateCharacterVGUI(filePath, "s_panel_biker", "s_panel_biker_incap", "select_francis");
    }

    // ReSharper disable once InconsistentNaming
    private static void CreateCharacterVGUI(string vtffile, string vgui1, string vgui2, string vgui3)
    {
        if (!File.Exists(vtffile)) throw new FileNotFoundException("VTF file not found.", vtffile);

        var dir = Path.GetDirectoryName(vtffile);
        if (dir == null) throw new DirectoryNotFoundException("Directory not found for the given VTF file.");

        File.Copy(vtffile, Path.Combine(dir, vgui1 + ".vtf"), true);
        File.Copy(vtffile, Path.Combine(dir, vgui2 + ".vtf"), true);
        File.Move(vtffile, Path.Combine(dir, vgui3 + ".vtf"), true);
    }
}