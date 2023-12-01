namespace Identity.Consumer.Models.Passkeys.Registration;

public class RegisterPasskeyResponseUser
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    
    public RegisterPasskeyResponseUser(string id, string name, string displayName)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
    }
}

public class RegisterPasskeyConfiguration
{
    public string Challenge { get; set; }

    public RegisterPasskeyResponseUser User { get; set; }

    public RegisterPasskeyConfiguration(string challenge, RegisterPasskeyResponseUser user)
    {
        Challenge = challenge;
        User = user;
    }
}