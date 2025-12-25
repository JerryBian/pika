using Pika.Common.Model;

namespace Pika.Models
{
    public class PikaScriptRunViewModel
    {
        public PikaScript Task { get; set; }

        public PikaScriptRun Run { get; set; }
    }
}
