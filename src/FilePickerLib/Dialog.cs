using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FilePicker;

public class Dialog
{
   public static async Task<string?> PickFileAsync(string title="Select a file") {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return PickFileWindows(title);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return await PickFileLinux(title);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return await PickFileOsx(title);
        throw new PlatformNotSupportedException();
    } 

#if WINDOWS
    private static string? PickFileWindows(string title) {
        
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Title = title,
            CheckFileExists = true,
            CheckPathExists = true
        };
        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
    }
# else
    private static string? PickFileWindows(string title) => null;
#endif

    private static Task<string?> PickFileLinux(string title) {
        
        //var completionSource = new TaskCompletionSource<string?>();
        var psi = new ProcessStartInfo
        {
            FileName = "zenity",
            Arguments = $"--file-selection --title=\"{title}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process == null) return Task.FromResult<string?>(null);
        
        process.WaitForExit();
        if (process.ExitCode == 0) {
            
            string output = process.StandardOutput.ReadToEnd().Trim();
            return Task.FromResult<string?>(output.Length > 0 ? output : null);
        }
        return Task.FromResult<string?>(null);
    }

    private static Task<string?> PickFileOsx(string title) {

        var psi = new ProcessStartInfo
        {
            FileName = "osascript",
            Arguments = $"-e 'POSIX path of (choose file with prompt \"{title}\")'",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process == null) return Task.FromResult<string?>(null);

        process.WaitForExit();
        if (process.ExitCode == 0) {
            
            string output = process.StandardOutput.ReadToEnd().Trim();
            return Task.FromResult<string?>(output.Length > 0 ? output : null);
        }
        return Task.FromResult<string?>(null);
    }
}

class Test {
    static async Task Main() {
        string? path = await Dialog.PickFileAsync("Select a file");
        if (path == null) Console.WriteLine("No file selected");
        else Console.WriteLine("Seleted path:\n" + path);
    }
}
