using System;
using System.Threading.Tasks;

namespace Pika.Lib.Command;

public interface ICommandClient : IAsyncDisposable
{
    void Stop();

    Task RunAsync(
        string shellName,
        string shellOption,
        string shellExt,
        string script,
        Func<string, Task> outputHandler = null,
        Func<string, Task> errorHandler = null,
        Func<Task> stopHandler = null);
}