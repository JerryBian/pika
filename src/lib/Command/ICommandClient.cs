using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pika.Lib.Command;

public interface ICommandClient
{
    Task<int> RunAsync(string script, Func<string, Task> outputHandler = null,
        Func<string, Task> errorHandler = null, CancellationToken cancellationToken = default);
}