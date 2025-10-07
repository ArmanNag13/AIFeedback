using AIFeedback.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using YourProject.Entities;

namespace AIFeedback.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerFeedback>()
                .Property(cf => cf.SenderEmail)
                .IsRequired();

            modelBuilder.Entity<CustomerFeedback>()
                .Property(cf => cf.Message)
                .IsRequired();
        }
    }
}
