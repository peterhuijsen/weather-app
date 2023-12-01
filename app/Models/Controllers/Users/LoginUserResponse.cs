namespace App.Models.Controllers.Users;

public class LoginUserResponse
{
    public string Token { get; set; } = null!;
    public User User { get; set; } = null!;
}