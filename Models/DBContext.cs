using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CAT.Models
{
    public class DBContext : IdentityDbContext<User>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        public DbSet<Answers> Answers { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Exam> Exam { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Users)
                .WithMany(u => u.Exams)
                .HasForeignKey(e => e.UserID);

            modelBuilder.Entity<Answers>()
                .HasOne(a => a.Questions)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionID);

            modelBuilder.Entity<Questions>().ToTable("Questions");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
        }
    }
}

