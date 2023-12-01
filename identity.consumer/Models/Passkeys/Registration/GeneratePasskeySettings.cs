namespace Identity.Consumer.Models.Passkeys.Registration;

public class GenerateAssertionPasskeySettings : GeneratePasskeySettings
{
    public new static GeneratePasskeySettingsBuilder<GenerateAssertionPasskeySettings> Create(string state)
        => new GeneratePasskeySettingsBuilder<GenerateAssertionPasskeySettings>(new GenerateAssertionPasskeySettings
        {
            State = state
        });

    public string State { get; set; }
}

public class GenerateAttestationPasskeySettings : GeneratePasskeySettings
{
    public new static GeneratePasskeySettingsBuilder<GenerateAttestationPasskeySettings> Create(Guid uuid, string name, string displayName)
        => new GeneratePasskeySettingsBuilder<GenerateAttestationPasskeySettings>(new GenerateAttestationPasskeySettings
        {
            Uuid = uuid,
            Name = name,
            DisplayName = displayName
        });

    public Guid Uuid { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
}

public class GeneratePasskeySettings
{
    public static GeneratePasskeySettingsBuilder<GenerateAttestationPasskeySettings> Create(Guid uuid, string name, string displayName)
        => GenerateAttestationPasskeySettings.Create(uuid, name, displayName);

    public static GeneratePasskeySettingsBuilder<GenerateAssertionPasskeySettings> Create(string state)
        => GenerateAssertionPasskeySettings.Create(state);
}

public class GeneratePasskeySettingsBuilder<T>
    where T : GeneratePasskeySettings
{
    private readonly T _instance;

    public GeneratePasskeySettingsBuilder(T instance)
        => _instance = instance;

    public T Build()
        => _instance;
}