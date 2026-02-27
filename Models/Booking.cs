using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class Booking
{
    public int BookingId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TutorId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    [ForeignKey("StudentId")]
    public Student Student { get; set; } = null!;

    [ForeignKey("TutorId")]
    public Tutor Tutor { get; set; } = null!;

    public SessionNote? SessionNote { get; set; }
}