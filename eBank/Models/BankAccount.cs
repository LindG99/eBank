using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eBank.Models
{
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SEK";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key to the user
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
