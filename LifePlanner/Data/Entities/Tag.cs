using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;


        // UTILIZADOR
        [Required]
        public string UserId { get; set; } = string.Empty;

        public User? User { get; set; }


        // RELAÇÃO N:N COM TAREFAS
        public ICollection<TaskTag> TaskTags { get; set; }
            = new List<TaskTag>();
    }
}
