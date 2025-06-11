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

            builder.Entity<Course>()
                .HasMany(c => c.EnrolledStudents)
                .WithMany()
                .UsingEntity<CourseEnrollment>(
                    j => j
                        .HasOne(ce => ce.User)
                        .WithMany() 
                        .HasForeignKey(ce => ce.UserId),
                    j => j
                        .HasOne(ce => ce.Course)
                        .WithMany() 
                        .HasForeignKey(ce => ce.CourseId),
                    j =>
                    {
                        j.HasKey(ce => ce.Id);
                        j.Property(ce => ce.Id).ValueGeneratedOnAdd(); 
                    }
                );
        }
    }
}