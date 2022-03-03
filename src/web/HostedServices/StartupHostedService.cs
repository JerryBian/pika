using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pika.Lib;
using Pika.Lib.Model;
using Pika.Lib.Store;

namespace Pika.Web.HostedServices
{
    public class StartupHostedService : BackgroundService
    {
        private readonly Setting _setting;
        private readonly IDbRepository _repository;

        public StartupHostedService(Setting setting, IDbRepository dbRepository)
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
            InitSetting();
            await _repository.StartupAsync();
            var runningTasks =
                await _repository.GetTaskRunsAsync(int.MaxValue, 0, $"status={(int) PikaTaskStatus.Running}");
            foreach (var runningTask in runningTasks)
            {
                await _repository.AddTaskRunOutputAsync(new PikaTaskRunOutput
                    {IsError = true, Message = "Flag as dead during startup.", TaskRunId = runningTask.Id});
                await _repository.UpdateTaskRunStatusAsync(runningTask.Id, PikaTaskStatus.Dead);
            }

            await base.StartAsync(cancellationToken);
        }

        private void InitSetting()
        {
            if (OperatingSystem.IsWindows())
            {
                _setting.Shell = "cmd.exe";
                _setting.ShellOptions = "/q /c";
                _setting.ShellScriptExt = "bat";
            }
            else if (OperatingSystem.IsLinux())
            {
                _setting.Shell = "bash";
                _setting.ShellOptions = "";
                _setting.ShellScriptExt = "sh";
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
