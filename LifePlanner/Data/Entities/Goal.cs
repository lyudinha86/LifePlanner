using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class Goal
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Status { get; set; } = "Em progresso";

        // Relação 1:N
        public ICollection<TaskItem> TaskItems { get; set; }
            = new List<TaskItem>();
    }
}
