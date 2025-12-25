using Pika.Common.Model;

namespace Pika.Models
{
    public class PikaScriptDetailViewModel
    {
        public PikaScript Script { get; set; }

        public List<PikaScriptRun> Runs { get; set; } = [];
    }
}
