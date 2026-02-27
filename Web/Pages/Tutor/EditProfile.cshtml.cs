using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TutorBookingApp.Pages.Tutor
{
    public class EditProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public int MinGradeLevel { get; set; }

        [BindProperty]
        [Required]
        public int MaxGradeLevel { get; set; }

        [BindProperty]
        [Required]
        public int LessonDurationMinutes { get; set; }

        [BindProperty]
        [Phone]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Bio { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

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

            Subject = tutor.Subject;
            MinGradeLevel = tutor.MinGradeLevel;
            MaxGradeLevel = tutor.MaxGradeLevel;
            LessonDurationMinutes = tutor.LessonDurationMinutes;
            Phone = tutor.Phone;
            Bio = tutor.Bio;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

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

            if (MinGradeLevel > MaxGradeLevel)
            {
                ErrorMessage = "Min grade level cannot be greater than max grade level.";
                return Page();
            }

            tutor.Subject = Subject;
            tutor.MinGradeLevel = MinGradeLevel;
            tutor.MaxGradeLevel = MaxGradeLevel;
            tutor.LessonDurationMinutes = LessonDurationMinutes;
            tutor.Phone = Phone;
            tutor.Bio = Bio;

            await _context.SaveChangesAsync();

            SuccessMessage = "Profile updated successfully!";

            return Page();
        }
    }
}
