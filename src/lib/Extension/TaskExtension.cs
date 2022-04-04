using System;
using System.Threading.Tasks;

namespace Pika.Lib.Extension;

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