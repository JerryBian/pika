namespace Pika.Common.Model
{
    public class PikaDriveTable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Size { get; set; }

        public string Type { get; set; }

        public string Model { get; set; }

        public string Serial { get; set; }

        public string Tran { get; set; }

        public long CreatedAt { get; set; }

        public List<PikaDrivePartitionTable> Partitions { get; set; } = new();
    }
}
