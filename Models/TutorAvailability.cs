using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class TutorAvailability
{
    [Key]
    public int AvailabilityId { get; set; }

    [Required]
    public int TutorId { get; set; }

    [Required]
    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [ForeignKey("TutorId")]
    public Tutor Tutor { get; set; } = null!;
}