using System.ComponentModel.DataAnnotations;

namespace eBank.Models
{
    public class TransferHistory
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int FromAccountId { get; set; }

        [Required]
        public int ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public virtual BankAccount FromAccount { get; set; }
        public virtual BankAccount ToAccount { get; set; }
    }
}
