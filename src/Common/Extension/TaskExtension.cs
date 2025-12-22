using System;
using System.Threading.Tasks;

namespace Pika.Common.Extension;

public static class TaskExtension
{
    public static async Task OkForCancel(this Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }
}