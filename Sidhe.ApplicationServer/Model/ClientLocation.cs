using Newtonsoft.Json;

namespace Sidhe.ApplicationServer.Model
{
    public class ClientLocation
    {
        [JsonIgnore] public uint LocationId { get; set; }
        [JsonProperty("userId")] public int UserId { get; set; }
        [JsonProperty("x")] public int X { get; set; }
        [JsonProperty("y")] public int Y { get; set; }
    }
}
