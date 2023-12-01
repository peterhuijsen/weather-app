using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Identity.Consumer.Models.Code;

[Table("OneTimeCredentials")]
public class OneTimeCredential
{
    /// <summary>
    /// The unique id of the credentials, to make the credentials unique even if
    /// the Google id is identical.
    /// </summary>
    [Key]
    [JsonIgnore]
    public Guid Uuid { get; set; }
    
    public List<HashOneTimeCredential> HashCodes { get; set; } = new();
    public List<TimeOneTimeCredential> TimeCodes { get; set; } = new();
}

[Table("HashOneTimeCredentials")]
public class HashOneTimeCredential
{
    [Key]
    [JsonIgnore]
    public Guid Uuid { get; set; }

    [JsonIgnore]
    public string Secret { get; set; } = null!;
    
    public long Counter { get; set; }
}

[Table("TimeOneTimeCredentials")]
public class TimeOneTimeCredential
{
    [Key]
    [JsonIgnore]
    public Guid Uuid { get; set; }

    [JsonIgnore]
    public string Secret { get; set; } = null!;
}