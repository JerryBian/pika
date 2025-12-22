namespace Pika.Common;

public class PikaOptions
{
    public string DbLocation { get; set; }

    public bool ForceRecreateDb { get; set; }

    public string AdminUserName { get; set; } = "test";

    public string AdminPassword { get; set; } = "testtest";
}