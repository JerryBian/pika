using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Pika.Common.Model;
using Pika.Common.Store;

namespace Pika.Hubs
{
    [Route("/hub/script")]
    public class ScriptHub : Hub
    {
        private readonly IPikaStore _repository;

        public ScriptHub(IPikaStore repository)
        {
            _repository = repository;
        }

        public async Task AddScriptAsync(PikaScript task)
        {
            //await _repository.AddScriptAsync(task);
        }

        //public async IAsyncEnumerable<int> Counter(
        //    [EnumeratorCancellation]
        //    CancellationToken cancellationToken)
        //{
        //    //for (var i = 0; i < count; i++)
        //    //{
        //    //    // Check the cancellation token regularly so that the server will stop
        //    //    // producing items if the client disconnects.
        //    //    cancellationToken.ThrowIfCancellationRequested();

        //    //    yield return i;

        //    //    // Use the cancellationToken in other APIs that accept cancellation
        //    //    // tokens so the cancellation can flow down to them.
        //    //    await Task.Delay(delay, cancellationToken);
        //    //}
        //}
    }
}
