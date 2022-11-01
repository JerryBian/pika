﻿using Microsoft.Extensions.Hosting;
using Pika.Lib.Model;
using Pika.Lib.Store;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pika.Web.HostedServices;

public class StartupHostedService : BackgroundService
{
    private readonly IDbRepository _repository;
    private readonly PikaSetting _setting;

    public StartupHostedService(IDbRepository dbRepository, PikaSetting setting)
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
        System.Collections.Generic.List<PikaTaskRun> runningTasks =
            await _repository.GetTaskRunsAsync(int.MaxValue, 0, $"status={(int)PikaTaskStatus.Running}");
        foreach (PikaTaskRun runningTask in runningTasks)
        {
            await _repository.AddTaskRunOutputAsync(new PikaTaskRunOutput
            {
                IsError = true,
                Message = "Flag as dead during startup.",
                TaskRunId = runningTask.Id,
                CreatedAt = DateTime.Now.Ticks
            });
            await _repository.UpdateTaskRunStatusAsync(runningTask.Id, PikaTaskStatus.Dead);
        }

        await base.StartAsync(cancellationToken);
    }

    private async Task InitSettingAsync()
    {
        string shellName = await _repository.GetSetting(SettingKey.ShellName);
        if (string.IsNullOrEmpty(shellName))
        {
            if (OperatingSystem.IsWindows())
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ShellName, "cmd.exe");
                await _repository.InsertOrUpdateSetting(SettingKey.ShellOptions, "/q /c");
                await _repository.InsertOrUpdateSetting(SettingKey.ShellExt, ".bat");
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ShellName, "/bin/bash");
                await _repository.InsertOrUpdateSetting(SettingKey.ShellOptions, "");
                await _repository.InsertOrUpdateSetting(SettingKey.ShellExt, ".sh");
            }
            else
            {
                throw new NotSupportedException("The platform is not supported yet.");
            }
        }

        string itemsPerPage = await _repository.GetSetting(SettingKey.ItemsPerPage);
        if (!int.TryParse(itemsPerPage, out int val) || val < 1)
        {
            await _repository.InsertOrUpdateSetting(SettingKey.ItemsPerPage, "8");
        }

        string retainSizeInMb = await _repository.GetSetting(SettingKey.RetainSizeInMb);
        if (!int.TryParse(retainSizeInMb, out int val2) || val2 < 1)
        {
            await _repository.InsertOrUpdateSetting(SettingKey.RetainSizeInMb, "200");
        }

        _setting.ItemsPerPage = Convert.ToInt32(await _repository.GetSetting(SettingKey.ItemsPerPage));
        _setting.RetainSizeInMb = Convert.ToInt32(await _repository.GetSetting(SettingKey.RetainSizeInMb));
        _setting.DefaultShellName = await _repository.GetSetting(SettingKey.ShellName);
        _setting.DefaultShellOption = await _repository.GetSetting(SettingKey.ShellOptions);
        _setting.DefaultShellExt = await _repository.GetSetting(SettingKey.ShellExt);
    }
}