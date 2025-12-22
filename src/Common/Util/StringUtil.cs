namespace Pika.Common.Util
{
    public static class StringUtil
    {
        public static string DisplayDiff(long? current, long? previous)
        {
            if (current == null || previous == null)
            {
                return "-";
            }
            var diff = current.Value - previous.Value;
            if (diff > 0)
            {
                return $"+{diff}";
            }
            else if (diff < 0)
            {
                return $"-{diff}";
            }
            else
            {
                return "0";
            }
        }

        public static string DisplayLong(long? val)
        {
            if (val == null)
            {
                return "-";
            }

            return val.Value.ToString("N0");
        }
    }
}
