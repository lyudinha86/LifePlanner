using System.ComponentModel.DataAnnotations;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LifePlanner.Models.ViewModels
{
    public class TaskFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public int? GoalId { get; set; }

        public IEnumerable<SelectListItem> Goals { get; set; }
            = new List<SelectListItem>();

        public List<int> SelectedTagIds { get; set; }
            = new List<int>();

        public IEnumerable<Tag> Tags { get; set; }
            = new List<Tag>();
    }
}