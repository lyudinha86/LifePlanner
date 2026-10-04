using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class PlannerEvent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }
    }
}
