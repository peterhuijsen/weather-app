namespace App.Models.Controllers.Users;

public class PasswordLoginUserParameters
{
    public string Email { get; set; } = null!;
    
    public string Password { get; set; } = null!;
}