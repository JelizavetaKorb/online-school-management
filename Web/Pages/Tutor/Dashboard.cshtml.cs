using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Tutor
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string CurrentUserName { get; set; } = string.Empty;
        public Models.Tutor? TutorProfile { get; set; }
        public List<Booking> UpcomingBookings { get; set; } = new();
        public List<Booking> PastBookings { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = HttpContext.Session.GetString("FullName") ?? "";

            TutorProfile = await _context.Tutors
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);

            if (TutorProfile == null)
            {
                return RedirectToPage("/Login");
            }

            var now = DateTime.Now;

            var upcomingBookings = await _context.Bookings
                .Include(b => b.Student)
                    .ThenInclude(s => s.User)
                .Include(b => b.SessionNote)
                .Where(b => b.TutorId == TutorProfile.TutorId && 
                           b.BookingDate >= now.Date)
                .ToListAsync();
            
            UpcomingBookings = upcomingBookings
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToList();

            var pastBookings = await _context.Bookings
                .Include(b => b.Student)
                    .ThenInclude(s => s.User)
                .Include(b => b.SessionNote)
                .Where(b => b.TutorId == TutorProfile.TutorId && 
                           b.BookingDate < now.Date)
                .ToListAsync();
            
            PastBookings = pastBookings
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Take(20)
                .ToList();

            return Page();
        }
    }
}