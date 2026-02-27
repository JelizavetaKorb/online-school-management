using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TutorBookingApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public string Role { get; set; } = "Student";

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userRole = Role == "Tutor" ? UserRole.Tutor : UserRole.Student;
            
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Tutor)
                .FirstOrDefaultAsync(u => u.FullName == FullName && u.Role == userRole);

            if (user == null)
            {
                ErrorMessage = "User not found. Please check your name and role.";
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", Role);
            HttpContext.Session.SetString("FullName", FullName);

            if (Role == "Student")
            {
                return RedirectToPage("/Student/SearchTutors");
            }
            else
            {
                return RedirectToPage("/Tutor/Dashboard");
            }
        }
    }
}