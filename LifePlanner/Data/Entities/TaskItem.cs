using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
