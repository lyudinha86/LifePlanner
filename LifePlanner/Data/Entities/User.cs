using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Data.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Nome completo")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
