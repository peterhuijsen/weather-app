using Identity.Consumer.Configuration;

namespace App.Settings;

public class Configuration
{
    public ModelConfiguration Model { get; set; } = new ModelConfiguration();

    public DatabaseConfiguration Database { get; set; } = new DatabaseConfiguration();

    public HashingConfiguration Hashing { get; set; } = new HashingConfiguration();
    
    public ClientConfiguration Client { get; set; } = new ClientConfiguration();
    
    public CredentialsConfiguration Credentials { get; set; } = new CredentialsConfiguration();

    public IdentityConsumerConfiguration Identity { get; set; } = new IdentityConsumerConfiguration();
    
    public TokenConfiguration Token { get; set; } = new TokenConfiguration();
}

public class ModelConfiguration
{
    public string Path { get; set; } = null!;
}

public class CredentialsConfiguration
{
    public GoogleCredentialsConfiguration Google { get; set; } = new GoogleCredentialsConfiguration();
    public MicrosoftCredentialsConfiguration Microsoft { get; set; } = new MicrosoftCredentialsConfiguration();
}

public class MicrosoftCredentialsConfiguration
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Callback { get; set; } = null!;
    
}

public class GoogleCredentialsConfiguration
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Callback { get; set; } = null!;
}

public class ClientConfiguration
{
    public string Url { get; set; } = null!;
    public string LoginUrl { get; set; } = null!;
}

public class DatabaseConfiguration
{
    public string Connection { get; set; } = null!;
}

public class HashingConfiguration
{
    public int Iterations { get; set; } = 100_000;
    
    public int Size { get; set; } = 128;
}

public class TokenConfiguration
{
    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;
    
    public string Secret { get; set; } = null!;

    public int Lifespan { get; set; } = 60 * 60 * 24;
}