using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class Tutor
{
    public int TutorId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int MinGradeLevel { get; set; }

    [Required]
    [Range(1, 5)]
    public int MaxGradeLevel { get; set; }

    [Required]
    public int LessonDurationMinutes { get; set; }

    [Phone]
    public string? Phone { get; set; }
    
    public string? Bio { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public ICollection<TutorAvailability> Availabilities { get; set; } = new List<TutorAvailability>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}