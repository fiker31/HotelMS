using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HotelDbContext _context;
        public EmployeeController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        public async Task<IActionResult> ListEmployees(string? search)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;

            var query = _context.Employees.Include(e => e.Account).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Name.Contains(search) || e.Position.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderBy(e => e.Name).ToListAsync());
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(Employee model)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Employee {model.Name} added successfully.";
            return RedirectToAction("ListEmployees");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(int id)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(Employee model)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            _context.Employees.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction("ListEmployees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;

            var hasReservations = await _context.Reservations.AnyAsync(r => r.EmployeeID == id);
            if (hasReservations)
            {
                TempData["Error"] = "Cannot delete employee with existing reservations.";
                return RedirectToAction("ListEmployees");
            }

            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee deleted.";
            }
            return RedirectToAction("ListEmployees");
        }

        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;

            var emp = await _context.Employees
                .Include(e => e.Account)
                .Include(e => e.Reservations).ThenInclude(r => r.Guest)
                .Include(e => e.Reservations).ThenInclude(r => r.Room)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(int id)
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;
            var emp = await _context.Employees.Include(e => e.Account).FirstOrDefaultAsync(e => e.EmployeeID == id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int employeeId, string role)
        {
            var auth = CheckAuth("Admin");
            if (auth != null) return auth;

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.EmployeeID == employeeId);
            if (account != null)
            {
                account.Role = role;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role updated.";
            }
            return RedirectToAction("ListEmployees");
        }
    }
}
