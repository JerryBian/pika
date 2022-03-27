using System.Text.Json.Serialization;

namespace Pika.Web.Models;

public class ApiResponse<T>
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("ok")]
    public bool IsOk { get; set; } = true;

    [JsonPropertyOrder(2)]
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("content")]
    public T Content { get; set; }

    [JsonPropertyOrder(4)]
    [JsonPropertyName("redirectTo")]
    public string RedirectTo { get; set; }
}