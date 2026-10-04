using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models.ViewModels
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
