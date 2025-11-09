using System;
using System.ComponentModel.DataAnnotations;

namespace eBank.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  

        [Required]
        public decimal OriginalAmount { get; set; }  

        [Required]
        public decimal RemainingAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false;
    }
}
