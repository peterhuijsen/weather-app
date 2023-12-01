using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Identity.Consumer.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Models;


[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public class User : IEntity
{
    [Key]
    public Guid Uuid { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    public bool Verified { get; set; } = false;
    
    public Credentials Credentials { get; set; } = null!;
}