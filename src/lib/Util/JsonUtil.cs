using Pika.Lib.Converter;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pika.Lib.Util;

public static class JsonUtil
{
    public static string Serialize<T>(T obj, bool writeIndented = false)
    {
        JsonSerializerOptions option = new()
        {
            WriteIndented = writeIndented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        option.Converters.Add(new IsoDateTimeConverter());
        return JsonSerializer.Serialize(obj, option);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }

    public static async Task<T> DeserializeAsync<T>(Stream stream)
    {
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }
}