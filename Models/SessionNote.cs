using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class SessionNote
{
    public int SessionNoteId { get; set; }

    [Required]
    public int BookingId { get; set; }

    [StringLength(50)]
    public string? Grade { get; set; }

    [StringLength(1000)]
    public string? ProgressDescription { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    [ForeignKey("BookingId")]
    public Booking Booking { get; set; } = null!;
}