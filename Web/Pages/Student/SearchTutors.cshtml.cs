using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace TutorBookingApp.Pages.Student
{
    public class SearchTutorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SearchTutorsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Models.Tutor> Tutors { get; set; } = new();
        public string? SearchSubject { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public int StudentGradeLevel { get; set; }

        public async Task<IActionResult> OnGetAsync(string? searchSubject)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            CurrentUserName = HttpContext.Session.GetString("FullName") ?? "";
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return RedirectToPage("/Login");
            }

            StudentGradeLevel = student.GradeLevel;
            SearchSubject = searchSubject;
            
            var query = _context.Tutors
                .Include(t => t.User)
                .Where(t => t.MinGradeLevel <= student.GradeLevel && 
                            t.MaxGradeLevel >= student.GradeLevel);

            if (!string.IsNullOrWhiteSpace(searchSubject))
            {
                query = query.Where(t => t.Subject.Contains(searchSubject));
            }

            Tutors = await query.ToListAsync();

            return Page();
        }
    }
}