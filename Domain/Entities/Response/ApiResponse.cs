using System.Text.Json.Serialization;

namespace Domain.Entities;

public class ApiResponse<T>
{
    [JsonPropertyName("Success")]
    public bool Success { get; set; }

    [JsonPropertyName("Data")]
    public T Data { get; set; }

    [JsonPropertyName("Errors")]
    public List<string> Errors { get; set; }

    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }


    public ApiResponse()
    {
        Errors = new List<string>();
    }

}

