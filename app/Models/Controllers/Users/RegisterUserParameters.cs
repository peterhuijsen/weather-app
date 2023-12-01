namespace App.Models.Controllers.Users;

public class RegisterUserParameters
{
    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    public string Password { get; set; } = null!;
}