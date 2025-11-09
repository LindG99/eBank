using eBank.Data;
using eBank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eBank.Controllers
{
    public class LoanController : Controller
    {
        // GET: Loan
        private readonly ILogger<LoanController> _logger;
        private readonly ApplicationDbContext _Context;
        private readonly LoanService _loanService = new LoanService();

        // Constructor
        public LoanController(ApplicationDbContext context, ILogger<LoanController> logger)
        {
            _Context = context;
            _logger = logger;
            _loanService = new LoanService();
        }
        //GET: Loan/Apply
        [HttpGet]
        public IActionResult Apply()
        {
            // Return the loan application view
            return View(new LoanApplicationViewModel());
        }

        //POST: Loan/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LoanApplicationViewModel model)
        {
            // Log the received application
            _logger.LogInformation("Ansökan mottagen: Income={Income}, Debt={Debt}", model.Income, model.Debt);

            // Validate the model
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors);
                }
                return View(model);
            }


            try
            {
                // Prepare input data for prediction
                var input = new LoanData
                {
                    Income = model.Income,
                    Debt = model.Debt,
                    CreditScore = model.CreditScore,
                    EmploymentYears = model.EmploymentYears,
                    LoanAmount = model.LoanAmount,
                    Approved = false
                };
                // Get prediction from the ML model
                var prediction = _loanService.Evaluate(input);
                // Update the model with prediction results
                model.Decision = prediction.Approved ? "Godkänt" : "Nekat";
                model.Probability = prediction.Probability;

                // if (prediction.Approved) add money to user's account
                if (prediction.Approved)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _logger.LogInformation("[LoanController] Användar-ID (Identity): {UserId}", userId);

                    var loan = new Loan
                    {
                        UserId = userId,
                        OriginalAmount = (decimal)model.LoanAmount,
                        RemainingAmount = (decimal)model.LoanAmount,
                        CreatedAt = DateTime.UtcNow,
                        IsPaid = false
                    };
                    _Context.Loans.Add(loan);
                    await  _Context.SaveChangesAsync();

                    // Find the user's bank account
                    var account = _Context.BankAccounts.FirstOrDefault(a => a.UserId == userId);
                    if (account != null)
                    {
                        account.Balance += (decimal)model.LoanAmount;
                        await _Context.SaveChangesAsync();
                        //Log the successful addition of funds
                        _logger.LogInformation("Lån godkänt och pengar tillagda till konto för användare {UserId}", userId);
                    }
                    else
                    {
                        //Log the failure to find the bank account
                        _logger.LogWarning("Kunde inte hitta bankkonto för användare {UserId}", userId);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hantering av låneansökan");
                model.Decision = "Fel vid prediktion";
                model.Probability = 0;
            }

            return View("Result", model);
        }
        // Get: Loan/MyLoans
        [HttpGet]
        public async Task<IActionResult> MyLoans()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var loans = await _Context.Loans
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(loans);
        }
        // Post: Loan/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int loanId, decimal amount)
        {
            // Find the loan
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var loan = await _Context.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
            if (loan == null || loan.IsPaid)
                return NotFound();
            // Find the user's bank account
            var account = await _Context.BankAccounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null || account.Balance < amount)
            {
                ModelState.AddModelError("", "Otillräckligt saldo på bankkontot.");
                return RedirectToAction("MyLoans");
            }
            // Validate payment amount
            if (amount <= 0 || amount > loan.RemainingAmount)
            {
                ModelState.AddModelError("", "Ogiltigt betalningsbelopp.");
                return RedirectToAction("MyLoans");
            }
            // Process payment
            account.Balance -= amount;
            loan.RemainingAmount -= amount;
            if (loan.RemainingAmount <= 0)
            {
                loan.IsPaid = true;
            }
            await _Context.SaveChangesAsync();
            return RedirectToAction("MyLoans");
        }

        //Print Loan Decision
        public IActionResult PrintLoanDecision(decimal loanAmount, string decision, double probability)
        {
            var userName = User.Identity?.Name ?? "Okänd användare";

            try
            {
                var pdfBytes = GenerateLoanDecisionPdf(userName, loanAmount, decision, probability);
                return File(pdfBytes, "application/pdf", $"Lanebeslut_{DateTime.Now:yyyyMMddHHmm}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid PDF-generering");
                return BadRequest("Kunde inte generera PDF.");
            }

        }
        // Generate PDF for Loan Decision
        private byte[] GenerateLoanDecisionPdf(string userName, decimal loanAmount, string decision, double probability)
        {
            using var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Lånebeslut - eBank").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Datum: {DateTime.Now:yyyy-MM-dd}");
                        col.Item().Text($"Namn: {userName}");
                        col.Item().Text($"Lånebelopp: {loanAmount:C}");
                        col.Item().Text($"Status: {(decision == "Godkänt" ? "✅ Godkänt" : "❌ Nekat")}");
                        col.Item().Text($"Sannolikhet: {(probability * 100):F2}%");
                        col.Spacing(15);
                        col.Item().Text("Tack för att du använder eBank!").Italic();
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
        
    }
}

