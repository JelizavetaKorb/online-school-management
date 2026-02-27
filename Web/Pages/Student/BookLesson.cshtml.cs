using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Student
{
    public class BookLessonModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BookLessonModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Tutor? Tutor { get; set; }
        public List<TutorAvailability> TutorAvailabilities { get; set; } = new();
        public List<string> AvailableSlots { get; set; } = new();

        [BindProperty]
        public int TutorId { get; set; }

        [BindProperty]
        public DateTime? BookingDate { get; set; }

        [BindProperty]
        public string? SelectedTimeSlot { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int tutorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            TutorId = tutorId;
            await LoadTutorData();

            return Page();
        }

        public async Task<IActionResult> OnPostCheckAvailabilityAsync()
        {
            await LoadTutorData();

            if (!BookingDate.HasValue)
            {
                ErrorMessage = "Please select a date.";
                return Page();
            }

            var dayOfWeek = (int)BookingDate.Value.DayOfWeek;
            var availability = TutorAvailabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

            if (availability == null)
            {
                ErrorMessage = $"Tutor is not available on {BookingDate.Value.DayOfWeek}s.";
                return Page();
            }

            var existingBookings = await _context.Bookings
                .Where(b => b.TutorId == TutorId && b.BookingDate.Date == BookingDate.Value.Date)
                .ToListAsync();

            AvailableSlots = GenerateTimeSlots(availability, existingBookings, Tutor!.LessonDurationMinutes);

            if (!AvailableSlots.Any())
            {
                ErrorMessage = "No available time slots for this date.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBookAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            await LoadTutorData();

            if (!BookingDate.HasValue || string.IsNullOrEmpty(SelectedTimeSlot))
            {
                ErrorMessage = "Please select both date and time slot.";
                return Page();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId.Value);
            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            var timeParts = SelectedTimeSlot.Split('-');
            var startTime = TimeSpan.Parse(timeParts[0].Trim());
            var endTime = TimeSpan.Parse(timeParts[1].Trim());

            var existingBookings = await _context.Bookings
                .Where(b => b.TutorId == TutorId && 
                            b.BookingDate.Date == BookingDate.Value.Date)
                .ToListAsync();
    
            var conflictingBooking = existingBookings
                .Any(b => b.StartTime < endTime && b.EndTime > startTime);

            if (conflictingBooking)
            {
                ErrorMessage = "This time slot is no longer available. Please select another.";
                return Page();
            }

            var booking = new Booking
            {
                StudentId = student.StudentId,
                TutorId = TutorId,
                BookingDate = BookingDate.Value,
                StartTime = startTime,
                EndTime = endTime,
                CreatedDate = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Lesson booked successfully for {BookingDate.Value.ToShortDateString()} at {startTime}!";
    
            return Page();
        }

        private async Task LoadTutorData()
        {
            Tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TutorId == TutorId);

            TutorAvailabilities = await _context.TutorAvailabilities
                .Where(a => a.TutorId == TutorId)
                .OrderBy(a => a.DayOfWeek)
                .ToListAsync();
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
