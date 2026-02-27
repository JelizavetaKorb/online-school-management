using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Student
{
    public class MyLessonsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MyLessonsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Booking> UpcomingLessons { get; set; } = new();
        public List<Booking> PastLessons { get; set; } = new();

        [BindProperty]
        public int BookingId { get; set; }

        [BindProperty]
        public DateTime? NewBookingDate { get; set; }

        [BindProperty]
        public string? NewTimeSlot { get; set; }

        public List<string> AvailableSlots { get; set; } = new();
        public Booking? BookingToReschedule { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            await LoadLessons(student.StudentId);

            return Page();
        }

        public async Task<IActionResult> OnGetRescheduleAsync(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            BookingToReschedule = await _context.Bookings
                .Include(b => b.Tutor)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.StudentId == student.StudentId);

            if (BookingToReschedule == null)
            {
                return RedirectToPage();
            }

            BookingId = bookingId;
            await LoadLessons(student.StudentId);

            return Page();
        }

        public async Task<IActionResult> OnPostCheckAvailabilityAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            BookingToReschedule = await _context.Bookings
                .Include(b => b.Tutor)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(b => b.BookingId == BookingId && b.StudentId == student.StudentId);

            if (BookingToReschedule == null)
            {
                return RedirectToPage();
            }

            if (!NewBookingDate.HasValue)
            {
                ErrorMessage = "Please select a date.";
                await LoadLessons(student.StudentId);
                return Page();
            }

            var dayOfWeek = (int)NewBookingDate.Value.DayOfWeek;
            var availability = await _context.TutorAvailabilities
                .FirstOrDefaultAsync(a => a.TutorId == BookingToReschedule.TutorId && 
                                         a.DayOfWeek == dayOfWeek);

            if (availability == null)
            {
                ErrorMessage = $"Tutor is not available on {NewBookingDate.Value.DayOfWeek}s.";
                await LoadLessons(student.StudentId);
                return Page();
            }

            var existingBookings = await _context.Bookings
                .Where(b => b.TutorId == BookingToReschedule.TutorId && 
                           b.BookingDate.Date == NewBookingDate.Value.Date &&
                           b.BookingId != BookingId)
                .ToListAsync();

            AvailableSlots = GenerateTimeSlots(availability, existingBookings, 
                                              BookingToReschedule.Tutor.LessonDurationMinutes);

            if (!AvailableSlots.Any())
            {
                ErrorMessage = "No available time slots for this date.";
            }

            await LoadLessons(student.StudentId);
            return Page();
        }

        public async Task<IActionResult> OnPostRescheduleAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            var booking = await _context.Bookings
                .Include(b => b.Tutor)
                .FirstOrDefaultAsync(b => b.BookingId == BookingId && b.StudentId == student.StudentId);

            if (booking == null)
            {
                ErrorMessage = "Booking not found.";
                await LoadLessons(student.StudentId);
                return Page();
            }

            if (!NewBookingDate.HasValue || string.IsNullOrEmpty(NewTimeSlot))
            {
                ErrorMessage = "Please select both date and time slot.";
                BookingToReschedule = booking;
                await LoadLessons(student.StudentId);
                return Page();
            }

            var timeParts = NewTimeSlot.Split('-');
            var startTime = TimeSpan.Parse(timeParts[0].Trim());
            var endTime = TimeSpan.Parse(timeParts[1].Trim());

            var existingBookings = await _context.Bookings
                .Where(b => b.TutorId == booking.TutorId && 
                           b.BookingDate.Date == NewBookingDate.Value.Date &&
                           b.BookingId != BookingId)
                .ToListAsync();

            var conflictingBooking = existingBookings
                .Any(b => b.StartTime < endTime && b.EndTime > startTime);

            if (conflictingBooking)
            {
                ErrorMessage = "This time slot is no longer available. Please select another.";
                BookingToReschedule = booking;
                await LoadLessons(student.StudentId);
                return Page();
            }

            booking.BookingDate = NewBookingDate.Value;
            booking.StartTime = startTime;
            booking.EndTime = endTime;

            await _context.SaveChangesAsync();

            SuccessMessage = $"Lesson rescheduled successfully to {NewBookingDate.Value.ToShortDateString()} at {startTime}!";
            await LoadLessons(student.StudentId);

            BookingToReschedule = null;
            BookingId = 0;
            NewBookingDate = null;
            NewTimeSlot = null;
            AvailableSlots.Clear();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.StudentId == student.StudentId);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                SuccessMessage = "Lesson cancelled successfully.";
            }

            await LoadLessons(student.StudentId);
            return Page();
        }

        private async Task LoadLessons(int studentId)
        {
            var now = DateTime.Now;

            var allBookings = await _context.Bookings
                .Include(b => b.Tutor)
                .ThenInclude(t => t.User)
                .Where(b => b.StudentId == studentId)
                .ToListAsync();

            UpcomingLessons = allBookings
                .Where(b => b.BookingDate > now.Date || 
                            (b.BookingDate == now.Date && b.StartTime > now.TimeOfDay))
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToList();

            PastLessons = allBookings
                .Where(b => b.BookingDate < now.Date || 
                            (b.BookingDate == now.Date && b.EndTime <= now.TimeOfDay))
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Take(10)
                .ToList();
        }

        private List<string> GenerateTimeSlots(TutorAvailability availability, 
                                               List<Booking> existingBookings, 
                                               int lessonDuration)
        {
            var slots = new List<string>();
            var currentTime = availability.StartTime;
            var lessonDurationTimeSpan = TimeSpan.FromMinutes(lessonDuration);

            while (currentTime.Add(lessonDurationTimeSpan) <= availability.EndTime)
            {
                var slotEnd = currentTime.Add(lessonDurationTimeSpan);
                var hasConflict = existingBookings.Any(b => 
                    (b.StartTime < slotEnd && b.EndTime > currentTime));

                if (!hasConflict)
                {
                    slots.Add($"{currentTime:hh\\:mm} - {slotEnd:hh\\:mm}");
                }

                currentTime = slotEnd;
            }

            return slots;
        }
    }
}