using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using crm.Server.Models;

namespace crm.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<ScheduleEvent> Events { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relacja wiele-do-wielu między Course a ApplicationUser przez CourseEnrollments
            builder.Entity<Course>()
                .HasMany(c => c.EnrolledStudents)
                .WithMany() // Bez właściwości nawigacyjnej po stronie ApplicationUser
                .UsingEntity<CourseEnrollment>(
                    j => j
                        .HasOne(ce => ce.User)
                        .WithMany() // Brak odwrotnej nawigacji
                        .HasForeignKey(ce => ce.UserId),
                    j => j
                        .HasOne(ce => ce.Course)
                        .WithMany() // Brak odwrotnej nawigacji
                        .HasForeignKey(ce => ce.CourseId),
                    j =>
                    {
                        j.HasKey(ce => ce.Id); // Klucz główny
                        j.Property(ce => ce.Id).ValueGeneratedOnAdd(); // Generowanie Id automatycznie
                    }
                );
        }
    }
}