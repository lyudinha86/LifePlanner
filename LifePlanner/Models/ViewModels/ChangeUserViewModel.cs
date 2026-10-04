using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models.ViewModels
{
    public class ChangeUserViewModel
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Username { get; set; } = string.Empty;
    }
}
