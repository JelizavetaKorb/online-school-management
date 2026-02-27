using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Tutor
{
    public class SessionNotesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SessionNotesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Booking? Booking { get; set; }

        [BindProperty]
        public int BookingId { get; set; }

        [BindProperty]
        public string? Grade { get; set; }

        [BindProperty]
        public string? ProgressDescription { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var tutor = await _context.Tutors
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);

            if (tutor == null)
            {
                return RedirectToPage("/Login");
            }

            BookingId = bookingId;

            Booking = await _context.Bookings
                .Include(b => b.Student)
                    .ThenInclude(s => s.User)
                .Include(b => b.SessionNote)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.TutorId == tutor.TutorId);

            if (Booking == null)
            {
                return RedirectToPage("/Tutor/Dashboard");
            }

            if (Booking.SessionNote != null)
            {
                Grade = Booking.SessionNote.Grade;
                ProgressDescription = Booking.SessionNote.ProgressDescription;
                Notes = Booking.SessionNote.Notes;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var tutor = await _context.Tutors
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);

            if (tutor == null)
            {
                return RedirectToPage("/Login");
            }

            Booking = await _context.Bookings
                .Include(b => b.Student)
                    .ThenInclude(s => s.User)
                .Include(b => b.SessionNote)
                .FirstOrDefaultAsync(b => b.BookingId == BookingId && b.TutorId == tutor.TutorId);

            if (Booking == null)
            {
                return RedirectToPage("/Tutor/Dashboard");
            }

            if (Booking.SessionNote == null)
            {
                var sessionNote = new SessionNote
                {
                    BookingId = BookingId,
                    Grade = Grade,
                    ProgressDescription = ProgressDescription,
                    Notes = Notes,
                    CreatedDate = DateTime.Now
                };

                _context.SessionNotes.Add(sessionNote);
            }
            else
            {
                Booking.SessionNote.Grade = Grade;
                Booking.SessionNote.ProgressDescription = ProgressDescription;
                Booking.SessionNote.Notes = Notes;
                Booking.SessionNote.CreatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            SuccessMessage = "Session notes saved successfully!";

            Booking = await _context.Bookings
                .Include(b => b.Student)
                    .ThenInclude(s => s.User)
                .Include(b => b.SessionNote)
                .FirstOrDefaultAsync(b => b.BookingId == BookingId);

            return Page();
        }
    }
}
