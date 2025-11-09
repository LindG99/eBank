using System.ComponentModel.DataAnnotations;

namespace eBank.Models
{
    public class TransferMoneyModel
    {
        [Required]
        [Display(Name = "från konto")]
        public int fromAccountId { get; set; }

        [Required]
        [Display(Name ="till konto")]
        public int toAccountId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Beloppet måste vara större än 0.")]
        [Display(Name = "Belopp")]
        public decimal Amount { get; set; }
    }
}
