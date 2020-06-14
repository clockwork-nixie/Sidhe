using Newtonsoft.Json;

namespace Sidhe.ApplicationServer.Model
{
    public class ClientDelta
    {
        [System.Text.Json.Serialization.JsonIgnore] public ClientDelta Next { get; set; }
        [JsonProperty("userId")] public int UserId { get; set; }
        [JsonProperty("x")] public int X { get; set;}
        [JsonProperty("y")] public int Y { get; set;}
    }
}
