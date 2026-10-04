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
    }
}
