using Newtonsoft.Json;

namespace Sidhe.ApplicationServer.Model
{
    public class ClientRequest
    {
        [JsonProperty("location")] public ClientLocation Location { get; set; }
    }
}
