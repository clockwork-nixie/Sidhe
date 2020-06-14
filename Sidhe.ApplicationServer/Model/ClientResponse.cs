using Newtonsoft.Json;

namespace Sidhe.ApplicationServer.Model
{
    public class ClientResponse
    {
        [JsonProperty("changes")] public ClientLocation[] Changes { get; set; }
        [JsonProperty("errorMessage")] public string ErrorMessage { get; set; }
        
        [JsonIgnore] public bool IsNecessary => (Changes?.Length ?? 0) > 0;
    }
}
