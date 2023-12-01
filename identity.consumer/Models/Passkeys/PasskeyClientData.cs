using Newtonsoft.Json;

namespace Identity.Consumer.Models.Passkeys;

public class PasskeyClientData
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("challenge")]
    public string Challenge { get; set; }

    [JsonProperty("origin")]
    public string Origin { get; set; }
}