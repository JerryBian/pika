using Pika.Lib.Model;
using System.Collections.Generic;

namespace Pika.Web.Models
{
    public class HomeViewModel
    {
        public List<PikaApp> Apps { get; } = new();

        public List<PikaTask> Tasks { get; } = new();

        public PikaSystemStatus SystemStatus { get; set; }
    }
}
