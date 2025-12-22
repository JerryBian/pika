using Humanizer;

namespace Pika.Common.Extension;

public static class HumanExtension
{
    public static string ToHuman(this DateTime time)
    {
        return time.Humanize(false);
    }

    public static string ToHuman(this TimeSpan interval)
    {
        return interval.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour);
    }

    public static string ToByteSizeHuman(this long bytes)
    {
        return bytes.Bytes().ToString("#.#");
    }
}