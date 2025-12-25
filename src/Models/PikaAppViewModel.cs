using Pika.Common.Model;

namespace Pika.Models
{
    public class PikaAppViewModel
    {
        public PikaApp App { get; set; }

        public string State { get; set; }

        public string StateClassName { get; set; }

        public string Url { get; set; }
    }
}
