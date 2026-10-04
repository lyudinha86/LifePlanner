using LifePlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> TaskItems { get; set; }

        public DbSet<Goal> Goals { get; set; }

        public DbSet<PlannerEvent> PlannerEvents { get; set; }

        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<TaskTag> TaskTags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new
                {
                    tt.TaskItemId,
                    tt.TagId
                });

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.TaskItem)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskItemId);

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId);
        }
    }
}
