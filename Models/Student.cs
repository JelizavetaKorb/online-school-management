using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class Student
{
    public int StudentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int GradeLevel { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string ParentEmail { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}