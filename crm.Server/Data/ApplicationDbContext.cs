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
        public DbSet<UserCourse> UserCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserCourse>()
                .HasKey(uc => new { uc.UserId, uc.CourseId });
            builder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(uc => uc.UserId);
            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(uc => uc.CourseId);
        }
    }
}