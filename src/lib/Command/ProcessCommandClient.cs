using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pika.Lib.Command;

public class ProcessCommandClient : ICommandClient
{
    private readonly Setting _setting;

    public ProcessCommandClient(Setting setting)
    {
        _setting = setting;
    }

    public async Task<int> RunAsync(string script, Func<string, Task> outputHandler = null,
        Func<string, Task> errorHandler = null,
        CancellationToken cancellationToken = default)
    {
        var scriptFile = await GetScriptFileAsync(script);
        using var process = new Process();
        using var resetEvent1 = new ManualResetEventSlim(false);
        using var resetEvent2 = new ManualResetEventSlim(false);
        process.StartInfo = new ProcessStartInfo(_setting.Shell, $"{_setting.ShellOptions} \"{scriptFile}\"")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        process.OutputDataReceived += async (sender, args) =>
        {
            if (args.Data == null)
            {
                resetEvent1.Set();
            }
            else
            {
                if (outputHandler != null)
                {
                    await outputHandler.Invoke(args.Data);
                }
            }
        };

        process.ErrorDataReceived += async (sender, args) =>
        {
            if (args.Data == null)
            {
                resetEvent2.Set();
            }
            else
            {
                if (errorHandler != null)
                {
                    await errorHandler.Invoke(args.Data);
                }
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);
        if (File.Exists(scriptFile))
        {
            File.Delete(scriptFile);
        }

        return process.Id;
    }

    private async Task<string> GetScriptFileAsync(string script)
    {
        var tempFile = Path.GetTempFileName();
        var scriptFile = $"{tempFile}.{_setting.ShellScriptExt}";
        await File.WriteAllTextAsync(scriptFile, script, new UTF8Encoding(false));
        return scriptFile;
    }
}