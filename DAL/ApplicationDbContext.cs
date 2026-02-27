using Microsoft.EntityFrameworkCore;
using Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Tutor> Tutors { get; set; }
    public DbSet<TutorAvailability> TutorAvailabilities { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<SessionNote> SessionNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.FullName, u.Role })
            .IsUnique();

        // User - Tutor one-to-one
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tutor)
            .WithOne(t => t.User)
            .HasForeignKey<Tutor>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - Student one-to-one
        modelBuilder.Entity<User>()
            .HasOne(u => u.Student)
            .WithOne(s => s.User)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tutor - TutorAvailability one-to-many
        modelBuilder.Entity<TutorAvailability>()
            .HasOne(ta => ta.Tutor)
            .WithMany(t => t.Availabilities)
            .HasForeignKey(ta => ta.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking - SessionNote one-to-one
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.SessionNote)
            .WithOne(sn => sn.Booking)
            .HasForeignKey<SessionNote>(sn => sn.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Student - Booking one-to-many
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Student)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Tutor - Booking one-to-many
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Tutor)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
