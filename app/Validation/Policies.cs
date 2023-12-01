namespace App.Validation;

public class Policies
{
    public const string MFA_REQUIRED_IF_ENABLED_POLICY = "UserMFARequired";
    public const string MFA_MISSING_POLICY = "UserMFANeeded";
}