using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Tutor
{
    public class ManageAvailabilityModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageAvailabilityModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TutorAvailability> Availabilities { get; set; } = new();

        [BindProperty]
        public int DayOfWeek { get; set; }

        [BindProperty]
        public TimeSpan StartTime { get; set; }

        [BindProperty]
        public TimeSpan EndTime { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        private int? TutorId { get; set; }

        public async Task<IActionResult> OnGetAsync()
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

            TutorId = tutor.TutorId;
            await LoadAvailabilities(tutor.TutorId);

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
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

            TutorId = tutor.TutorId;

            if (StartTime >= EndTime)
            {
                ErrorMessage = "End time must be after start time.";
                await LoadAvailabilities(tutor.TutorId);
                return Page();
            }

            var existingAvailabilities = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutor.TutorId && a.DayOfWeek == DayOfWeek)
                .ToListAsync();

            var hasOverlap = existingAvailabilities
                .Any(a => a.StartTime < EndTime && a.EndTime > StartTime);

            if (hasOverlap)
            {
                ErrorMessage = "This time slot overlaps with existing availability.";
                await LoadAvailabilities(tutor.TutorId);
                return Page();
            }

            var availability = new TutorAvailability
            {
                TutorId = tutor.TutorId,
                DayOfWeek = DayOfWeek,
                StartTime = StartTime,
                EndTime = EndTime
            };

            _context.TutorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            SuccessMessage = "Availability added successfully!";
            await LoadAvailabilities(tutor.TutorId);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int availabilityId)
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

            var availability = await _context.TutorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId && 
                                         a.TutorId == tutor.TutorId);

            if (availability != null)
            {
                _context.TutorAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
                SuccessMessage = "Availability removed successfully!";
            }

            await LoadAvailabilities(tutor.TutorId);

            return Page();
        }

        private async Task LoadAvailabilities(int tutorId)
        {
            Availabilities = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId)
                .ToListAsync();  // Load data first
    
            Availabilities = Availabilities
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToList();
        }
    }
}
