using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pika.Lib.Model;
using Pika.Lib.Store;

namespace Pika.Lib.Command;

public class ProcessCommandClient : ICommandClient
{
    private readonly ILogger<ProcessCommandClient> _logger;
    private readonly IDbRepository _repository;

    public ProcessCommandClient(IDbRepository repository, ILogger<ProcessCommandClient> logger)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<int> RunAsync(string script, Func<string, Task> outputHandler = null,
        Func<string, Task> errorHandler = null,
        CancellationToken cancellationToken = default)
    {
        var scriptFile = await GetScriptFileAsync(script);
        using var process = new Process();
        using var resetEvent1 = new ManualResetEventSlim(false);
        using var resetEvent2 = new ManualResetEventSlim(false);
        var shellName = await _repository.GetSetting(SettingKey.ShellName);
        var shellOptions = await _repository.GetSetting(SettingKey.ShellOptions);
        process.StartInfo = new ProcessStartInfo(shellName, $"{shellOptions} \"{scriptFile}\"")
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
        _logger.LogDebug($"Process: {process.Id} ==> {process.StartInfo.FileName} {process.StartInfo.Arguments}");

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);
        if (File.Exists(scriptFile))
        {
            File.Delete(scriptFile);
        }

        _logger.LogDebug($"Process {process.Id} completed ...");
        return process.Id;
    }

    private async Task<string> GetScriptFileAsync(string script)
    {
        var tempFile = Path.GetTempFileName();
        var shellExt = await _repository.GetSetting(SettingKey.ShellExt);
        var scriptFile = $"{tempFile}{shellExt}";
        await File.WriteAllTextAsync(scriptFile, script, new UTF8Encoding(false));
        return scriptFile;
    }
}