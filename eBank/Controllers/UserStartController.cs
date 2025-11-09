using eBank.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBank.Controllers
{
    // Controller accessible only to users with the "User" role
    [Authorize(Roles = "User")]
    [Route("User")]
    // Controller to handle user start page and related actions
    public class UserStartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public UserStartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: User/UserStart
        [Route("UserStart")]
        public async Task<IActionResult> UserStart()
        {
            var user = await _userManager.GetUserAsync(User);

            var accounts = await _context.BankAccounts
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            return View("/Views/User/UserStart.cshtml", accounts);
        }

    }
}
