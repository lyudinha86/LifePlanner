using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifePlanner.Data.Entities
{
    public class FinancialTransaction
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Category { get; set; }
    }
}
