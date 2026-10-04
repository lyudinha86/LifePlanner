using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Relação N:N com TaskItem
        public ICollection<TaskTag> TaskTags { get; set; }
            = new List<TaskTag>();
    }
}
