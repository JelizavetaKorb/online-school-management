using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.ComponentModel.DataAnnotations;

namespace TutorBookingApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Role { get; set; } = "Student";

        [BindProperty]
        public int? GradeLevel { get; set; }

        [BindProperty]
        [EmailAddress]
        public string? ParentEmail { get; set; }

        [BindProperty]
        public string? Subject { get; set; }

        [BindProperty]
        public int? MinGradeLevel { get; set; }

        [BindProperty]
        public int? MaxGradeLevel { get; set; }

        [BindProperty]
        public int? LessonDurationMinutes { get; set; }

        [BindProperty]
        [Phone]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Bio { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userRole = Role == "Tutor" ? UserRole.Tutor : UserRole.Student;
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.FullName == FullName && u.Role == userRole);

            if (existingUser != null)
            {
                ErrorMessage = $"A {Role.ToLower()} with this name already exists.";
                return Page();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    FullName = FullName,
                    Role = userRole
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (Role == "Student")
                {
                    if (!GradeLevel.HasValue || string.IsNullOrEmpty(ParentEmail))
                    {
                        ErrorMessage = "Grade level and parent email are required for students.";
                        return Page();
                    }

                    var student = new Models.Student
                    {
                        UserId = user.UserId,
                        GradeLevel = GradeLevel.Value,
                        ParentEmail = ParentEmail
                    };
                    _context.Students.Add(student);
                }
                else
                {
                    if (string.IsNullOrEmpty(Subject) || !MinGradeLevel.HasValue ||
                        !MaxGradeLevel.HasValue || !LessonDurationMinutes.HasValue)
                    {
                        ErrorMessage = "Subject, grade levels, and lesson duration are required.";
                        return Page();
                    }

                    if (MinGradeLevel.Value > MaxGradeLevel.Value)
                    {
                        ErrorMessage = "Min grade level cannot be greater than max grade level.";
                        return Page();
                    }

                    var tutor = new Models.Tutor
                    {
                        UserId = user.UserId,
                        Subject = Subject,
                        MinGradeLevel = MinGradeLevel.Value,
                        MaxGradeLevel = MaxGradeLevel.Value,
                        LessonDurationMinutes = LessonDurationMinutes.Value,
                        Phone = Phone,
                        Bio = Bio
                    };
                    _context.Tutors.Add(tutor);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToPage("/Login");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ErrorMessage = "Registration failed. Please try again.";
                return Page();
            }
        }
    }
}
