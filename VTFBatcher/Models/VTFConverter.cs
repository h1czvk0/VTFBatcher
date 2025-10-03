using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VTFBatcher.Models;

public static class VTFConverter
{
    public static async Task<(string, string)> ConvertToVTF(string args, string inputFilePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "External\\VTFCmd.exe",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo);
        var ReadOutputTask = process?.StandardOutput.ReadToEndAsync();
        var ReadErrorTask = process?.StandardError.ReadToEndAsync();

        await process?.WaitForExitAsync();
        var output = await ReadOutputTask;

        Debug.WriteLine(output);
        // Debug.WriteLine(error);

        if (!String.IsNullOrEmpty(output) && output.Contains("Error"))
        {
            return (inputFilePath, output);
        }

        return (null, null);
    }

    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern bool vlInitialize();
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern bool vlCreateImage(out uint imageId);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern bool vlBindImage(uint imageId);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern void vlImageCreateDefaultCreateStructure(IntPtr options);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern bool vlImageCreateSingle(uint width, uint height, byte[] imageData, IntPtr options);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern bool vlImageSave(string filename);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern void vlDeleteImage(uint imageId);
    //
    // [DllImport("VTFLib.dll", CallingConvention = CallingConvention.Cdecl)]
    // public static extern void vlShutdown();
}