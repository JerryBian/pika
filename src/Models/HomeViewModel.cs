using Pika.Common.Model;
using System.Collections.Generic;

namespace Pika.Models
{
    public class HomeViewModel
    {
        public List<PikaApp> Apps { get; } = new();

        public List<PikaTask> Tasks { get; } = new();

        public PikaSystemStatus SystemStatus { get; set; }
    }
}
