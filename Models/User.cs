using System.ComponentModel.DataAnnotations;

namespace Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
    
    public Tutor? Tutor { get; set; }
    public Student? Student { get; set; }
}