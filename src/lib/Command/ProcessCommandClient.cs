using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pika.Lib.Extension;

namespace Pika.Lib.Command;

public class ProcessCommandClient : ICommandClient
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<ProcessCommandClient> _logger;
    private readonly Process _process;
    private readonly ManualResetEventSlim _resetEvent1;
    private readonly ManualResetEventSlim _resetEvent2;

    public ProcessCommandClient(ILogger<ProcessCommandClient> logger)
    {
        _logger = logger;
        _process = new Process();
        _resetEvent1 = new ManualResetEventSlim(false);
        _resetEvent2 = new ManualResetEventSlim(false);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task RunAsync(string shellName, string shellOption, string shellExt, string script,
        Func<string, Task> outputHandler = null,
        Func<string, Task> errorHandler = null, Func<Task> stopHandler = null)
    {
        var scriptFile = await GetScriptFileAsync(shellExt, script);
        _process.StartInfo = new ProcessStartInfo(shellName, $"{shellOption} \"{scriptFile}\"")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        _process.OutputDataReceived += async (_, args) =>
        {
            if (args.Data == null)
            {
                _resetEvent1.Set();
            }
            else
            {
                if (outputHandler != null)
                {
                    await outputHandler.Invoke(args.Data);
                }
            }
        };

        _process.ErrorDataReceived += async (_, args) =>
        {
            if (args.Data == null)
            {
                _resetEvent2.Set();
            }
            else
            {
                if (errorHandler != null)
                {
                    await errorHandler.Invoke(args.Data);
                }
            }
        };

        _process.Start();
        _logger.LogInformation(
            $"Process: {_process.Id} ==> {_process.StartInfo.FileName} {_process.StartInfo.Arguments}");

        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();
        await _process.WaitForExitAsync(_cancellationTokenSource.Token).OkForCancel();

        if (_cancellationTokenSource.IsCancellationRequested && stopHandler != null)
        {
            await stopHandler.Invoke();
        }

        if (File.Exists(scriptFile))
        {
            File.Delete(scriptFile);
        }

        _logger.LogInformation($"Process {_process.Id} completed ...");
    }

    public async ValueTask DisposeAsync()
    {
        _process?.Dispose();
        _resetEvent1?.Dispose();
        _resetEvent2?.Dispose();
        _cancellationTokenSource?.Dispose();
        await Task.CompletedTask;
    }

    private async Task<string> GetScriptFileAsync(string shellExt, string script)
    {
        var tempFile = Path.GetTempFileName();
        var scriptFile = $"{tempFile}{shellExt}";
        await File.WriteAllTextAsync(scriptFile, script, new UTF8Encoding(false));
        return scriptFile;
    }
}