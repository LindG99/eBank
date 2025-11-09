using Microsoft.AspNetCore.Mvc;

namespace eBank.Models
{
    public class LoanApplicationViewModel
    {
        public float Income { get; set; }
        public float Debt { get; set; }
        public float CreditScore { get; set; }
        public float EmploymentYears { get; set; }
        public float LoanAmount { get; set; }

        // Results
        public string? Decision { get; set; }
        public float Probability { get; set; }

        public string? TargetAccountName { get; set; }

    }
}
