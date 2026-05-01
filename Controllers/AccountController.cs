using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using HotelMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly HotelDbContext _context;
        public AccountController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (account == null || !AuthHelper.VerifyPassword(model.Password, account.Password))
            {
                ViewBag.Error = "Invalid username or password.";
                return View(model);
            }

            SessionHelper.SetUserID(HttpContext.Session, account.UserID);
            SessionHelper.SetUsername(HttpContext.Session, account.Username);
            SessionHelper.SetUserRole(HttpContext.Session, account.Role);

            if (account.Role == "Staff")
                return RedirectToAction("ViewRooms", "Room");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var auth = CheckAuth();
            if (auth != null) return auth;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            var userID = SessionHelper.GetUserID(HttpContext.Session);
            var account = await _context.Accounts.FindAsync(userID);
            if (account == null) return RedirectToAction("Login");

            if (!AuthHelper.VerifyPassword(model.CurrentPassword, account.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            account.Password = AuthHelper.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccount()
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;
            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _context.Employees.ToListAsync();
                return View(model);
            }

            if (await _context.Accounts.AnyAsync(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                ViewBag.Employees = await _context.Employees.ToListAsync();
                return View(model);
            }

            _context.Accounts.Add(new Account
            {
                Username = model.Username,
                Password = AuthHelper.HashPassword(model.Password),
                Role = model.Role,
                EmployeeID = model.EmployeeID
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Account created successfully.";
            return RedirectToAction("ListAllAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> ListAllAccounts()
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;
            var accounts = await _context.Accounts.Include(a => a.Employee).ToListAsync();
            return View(accounts);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;

            var account = await _context.Accounts.FindAsync(id);
            if (account != null && account.Username != "admin")
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ListAllAccounts");
        }
    }
}
