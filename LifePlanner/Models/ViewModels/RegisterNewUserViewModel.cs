using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models.ViewModels
{
    public class RegisterNewUserViewModel
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

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "A password e a confirmação não coincidem.")]
        [Display(Name = "Confirmar password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}