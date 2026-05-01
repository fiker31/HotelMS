using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class GuestController : Controller
    {
        private readonly HotelDbContext _context;
        public GuestController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        public async Task<IActionResult> ListAllGuests(string? search)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var query = _context.Guests.Include(g => g.Reservations).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(g => g.FirstName.Contains(search) ||
                                         g.LastName.Contains(search) ||
                                         g.Email.Contains(search) ||
                                         g.Phone.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderBy(g => g.LastName).ToListAsync());
        }

        public IActionResult ViewGuests(string? search) => RedirectToAction("ListAllGuests", new { search });

        [HttpGet]
        public IActionResult AddGuest()
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGuest(Guest model)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            _context.Guests.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Guest {model.FullName} added successfully.";
            return RedirectToAction("ListAllGuests");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateGuest(int id)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null) return NotFound();
            return View(guest);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGuest(Guest model)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            _context.Guests.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Guest updated successfully.";
            return RedirectToAction("ListAllGuests");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            var hasReservations = await _context.Reservations.AnyAsync(r => r.GuestID == id);
            if (hasReservations)
            {
                TempData["Error"] = "Cannot delete guest with existing reservations.";
                return RedirectToAction("ListAllGuests");
            }

            var guest = await _context.Guests.FindAsync(id);
            if (guest != null)
            {
                _context.Guests.Remove(guest);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Guest deleted successfully.";
            }
            return RedirectToAction("ListAllGuests");
        }
    }
}
