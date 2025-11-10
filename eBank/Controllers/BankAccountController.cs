using eBank.Data;
using eBank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace eBank.Controllers
{
    [Authorize]
    public class BankAccountController : Controller
    {
        // GET: BankAccount
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
    
        public BankAccountController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //List all bank accounts for the logged-in user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var accounts = _context.BankAccounts
                                   .Where(a => a.UserId == user.Id)
                                   .ToList();
            return View(accounts);
        }
        //Show form to create a new bank account
        public IActionResult Create()
        {
            return View();
        }

        //Handle form submission to create a new bank account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankAccount model)
        {
            // UserId & AccountNumber before validation
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Content("User ID could not be determined.");
            }

            model.UserId = userId;
            model.AccountNumber = GenerateAccountNumber();

            // Remove UserId and AccountNumber from ModelState to avoid validation errors
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.AccountNumber));

            if (!TryValidateModel(model))
            {
                // Log validation errors to console for debugging
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Fält: {state.Key}, Fel: {error.ErrorMessage}");
                    }
                }

                // Return detailed error messages
                var allErrors = ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage);
                return Content("ModelState fel: " + string.Join("; ", allErrors));
            }
            // Set default values
            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;
            // Initial balance
            _context.BankAccounts.Add(model);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        //Simple account number generator
        private string GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(10000000, 99999999).ToString(); // 8 number
        }

        // GET: BankAccount/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Find the bank account by id and user
            var user = await _userManager.GetUserAsync(User);
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            // If not found, return 404
            if (account == null)
            {
                return NotFound();
            }
            // Show confirmation view
            return View(account);
        }
        // POST: BankAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the bank account by id and user
            var user = await _userManager.GetUserAsync(User);
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            // If not found, return 404
            if (account == null)
            {
                return NotFound();
            }
            // Soft-delete: keep history intact but mark account inactive
            account.IsActive = false;
            _context.BankAccounts.Update(account);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //Get: BankAccount/Transfer
        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            // Load user's active bank accounts
            var user = await _userManager.GetUserAsync(User);
            var accounts = _context.BankAccounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .ToList();

            ViewBag.Accounts = accounts;
            return View(new TransferMoneyModel());
        }

        //POST: Bankaccount/Transfer
        [HttpPost]
        public async Task<IActionResult> Transfer(TransferMoneyModel model)
        {
            // Load user's active bank accounts
            var user = await _userManager.GetUserAsync(User);
            //get accounts for dropdown
            var accounts = _context.BankAccounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .ToList();

            ViewBag.Accounts = accounts; //return if error

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.fromAccountId == model.toAccountId)
            {
                ModelState.AddModelError("", "Du kan inte överföra till samma konto.");
                return View(model);
            }
            // Find the from and to accounts
            var fromAccount = accounts.FirstOrDefault(a => a.Id == model.fromAccountId);
            var toAccount = accounts.FirstOrDefault(a => a.Id == model.toAccountId);
            if (fromAccount == null || toAccount == null)
            {
                ModelState.AddModelError("", "Ogiltigt konto valt.");
                return View(model);
            }

            if (fromAccount.Balance < model.Amount)
            {
                ModelState.AddModelError("", "Otillräckligt saldo på från-kontot.");
                return View(model);
            }
            // Perform the transfer within a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                fromAccount.Balance -= model.Amount;
                toAccount.Balance += model.Amount;

                var history = new TransferHistory
                {
                    UserId = user.Id,
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = model.Amount,
                    Date = DateTime.UtcNow
                };

                _context.TransferHistories.Add(history);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ett fel uppstod vid överföringen. Försök igen.");
                return View(model);
            }
        }

        //Transfer history
        [HttpGet]
        public async Task<IActionResult> History()
        {
            // Load transfer history for the user
            var user = await _userManager.GetUserAsync(User);
            var history = await _context.TransferHistories
                .Where(h => h.UserId == user.Id)
                .Include(h => h.FromAccount)
                .Include(h => h.ToAccount)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
            return View(history);
        }
        //Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = await _context.TransferHistories
                .Where(t => t.UserId == userId && t.Date.Year == DateTime.Now.Year)
                .GroupBy(t => t.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount))
                })
                .ToListAsync();
            ViewBag.Months = transactions
        .Select(t => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(t.Month))
        .ToList();

            ViewBag.Incomes = transactions.Select(t => t.Income).ToList();
            ViewBag.Expenses = transactions.Select(t => t.Expense).ToList();

            // För en enkel summering
            ViewBag.TotalIncome = transactions.Sum(t => t.Income);
            ViewBag.TotalExpense = transactions.Sum(t => t.Expense);
            ViewBag.Net = ViewBag.TotalIncome - ViewBag.TotalExpense;

            return View();
        }
    }
}
