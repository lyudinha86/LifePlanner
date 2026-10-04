using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password atual")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword",
            ErrorMessage = "A nova password e a confirmação não coincidem.")]
        [Display(Name = "Confirmar nova password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
