using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class RoomController : Controller
    {
        private readonly HotelDbContext _context;
        public RoomController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        public async Task<IActionResult> ViewRooms(string? type, string? status)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var query = _context.Rooms.AsQueryable();
            if (!string.IsNullOrWhiteSpace(type)) query = query.Where(r => r.RoomType == type);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);

            ViewBag.FilterType = type;
            ViewBag.FilterStatus = status;
            return View(await query.OrderBy(r => r.RoomNumber).ToListAsync());
        }

        public async Task<IActionResult> ListAvailableRooms()
        {
            var auth = CheckAuth();
            if (auth != null) return auth;
            return View(await _context.Rooms.Where(r => r.Status == "Available")
                .OrderBy(r => r.RoomNumber).ToListAsync());
        }

        [HttpGet]
        public IActionResult AddRoom()
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(Room model)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == model.RoomNumber))
            {
                ModelState.AddModelError("RoomNumber", "Room number already exists.");
                return View(model);
            }

            _context.Rooms.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Room {model.RoomNumber} added successfully.";
            return RedirectToAction("ViewRooms");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateRoom(int id)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoom(Room model)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;
            if (!ModelState.IsValid) return View(model);

            var duplicate = await _context.Rooms.AnyAsync(r => r.RoomNumber == model.RoomNumber && r.RoomID != model.RoomID);
            if (duplicate)
            {
                ModelState.AddModelError("RoomNumber", "Room number already exists.");
                return View(model);
            }

            _context.Rooms.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Room updated successfully.";
            return RedirectToAction("ViewRooms");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var auth = CheckAuth("Admin", "Manager");
            if (auth != null) return auth;

            var hasReservations = await _context.Reservations.AnyAsync(r => r.RoomID == id);
            if (hasReservations)
            {
                TempData["Error"] = "Cannot delete room with existing reservations.";
                return RedirectToAction("ViewRooms");
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Room deleted successfully.";
            }
            return RedirectToAction("ViewRooms");
        }
    }
}
