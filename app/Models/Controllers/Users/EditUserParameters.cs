namespace App.Models.Controllers.Users;

public class EditUserParameters
{
    public string? Username { get; set; }
    public bool? MFA { get; set; }
}