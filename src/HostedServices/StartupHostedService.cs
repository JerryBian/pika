using Pika.Common.Model;
using Pika.Common.Store;

namespace Pika.HostedServices;

public class StartupHostedService : BackgroundService
{
    private readonly IPikaStore _repository;
    private readonly PikaSetting _setting;

    public StartupHostedService(IPikaStore dbRepository, PikaSetting setting)
    {
        _setting = setting;
        _repository = dbRepository;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _repository.StartupAsync();
        await InitSettingAsync();
        var runningTasks =
            await _repository.GetScriptRunsByStatusAsync(PikaScriptStatus.Running, false);
        foreach (var runningTask in runningTasks)
        {
            await _repository.AddScriptRunOutputAsync(new PikaScriptRunOutput
            {
                IsError = true,
                Message = "Flag as dead during startup.",
                TaskRunId = runningTask.Id,
                CreatedAt = DateTime.Now.Ticks
            });
            await _repository.UpdateScriptRunStatusAsync(runningTask.Id, PikaScriptStatus.Dead);
        }

        await base.StartAsync(cancellationToken);
    }

    private async Task InitSettingAsync()
    {
        var shellName = await _repository.GetSetting(PikaSettingKey.ShellName);
        if (string.IsNullOrEmpty(shellName))
        {
            if (OperatingSystem.IsWindows())
            {
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellName, "cmd.exe");
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellOptions, "/q /c");
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellExt, ".bat");
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellName, "/bin/bash");
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellOptions, "");
                await _repository.InsertOrUpdateSetting(PikaSettingKey.ShellExt, ".sh");
            }
            else
            {
                throw new NotSupportedException("The platform is not supported yet.");
            }
        }

        var itemsPerPage = await _repository.GetSetting(PikaSettingKey.ItemsPerPage);
        if (!int.TryParse(itemsPerPage, out var val) || val < 1)
        {
            await _repository.InsertOrUpdateSetting(PikaSettingKey.ItemsPerPage, "8");
        }

        var retainSizeInMb = await _repository.GetSetting(PikaSettingKey.RetainSizeInMb);
        if (!int.TryParse(retainSizeInMb, out var val2) || val2 < 1)
        {
            await _repository.InsertOrUpdateSetting(PikaSettingKey.RetainSizeInMb, "200");
        }

        _setting.RetainSizeInMb = Convert.ToInt32(await _repository.GetSetting(PikaSettingKey.RetainSizeInMb));
        _setting.DefaultShellName = await _repository.GetSetting(PikaSettingKey.ShellName);
        _setting.DefaultShellOption = await _repository.GetSetting(PikaSettingKey.ShellOptions);
        _setting.DefaultShellExt = await _repository.GetSetting(PikaSettingKey.ShellExt);
    }
}