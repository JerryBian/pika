using Pika.Common.Model;

namespace Pika.Models
{
    public class PikaScriptIndexViewModel
    {
        public List<PikaScript> SavedScripts { get; set; } = new List<PikaScript>();
    }
}
