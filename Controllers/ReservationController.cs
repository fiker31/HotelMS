using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using HotelMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class ReservationController : Controller
    {
        private readonly HotelDbContext _context;
        public ReservationController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        public async Task<IActionResult> ListAllReservations(string? search, DateTime? from, DateTime? to)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var query = _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.Employee)
                .Include(r => r.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.Guest.FirstName.Contains(search) || r.Guest.LastName.Contains(search));
            if (from.HasValue) query = query.Where(r => r.CheckInDate >= from.Value);
            if (to.HasValue) query = query.Where(r => r.CheckOutDate <= to.Value);

            ViewBag.Search = search;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            var list = await query.OrderByDescending(r => r.ReservationID).ToListAsync();
            return View(list.Select(r => new ReservationViewModel
            {
                ReservationID = r.ReservationID,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                GuestID = r.GuestID,
                GuestFullName = r.Guest.FullName,
                GuestPhone = r.Guest.Phone,
                RoomID = r.RoomID,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType,
                RoomPrice = r.Room.Price,
                EmployeeID = r.EmployeeID,
                EmployeeName = r.Employee.Name
            }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> CreateReservation()
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;
            ViewBag.Guests = await _context.Guests.OrderBy(g => g.LastName).ToListAsync();
            ViewBag.Rooms = await _context.Rooms.Where(r => r.Status == "Available").OrderBy(r => r.RoomNumber).ToListAsync();
            var empId = SessionHelper.GetUserID(HttpContext.Session);
            ViewBag.EmployeeID = await GetEmployeeIdForUser(empId);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(Reservation model)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            if (model.CheckOutDate <= model.CheckInDate)
                ModelState.AddModelError("CheckOutDate", "Check-out must be after check-in.");

            if (!ModelState.IsValid)
            {
                ViewBag.Guests = await _context.Guests.OrderBy(g => g.LastName).ToListAsync();
                ViewBag.Rooms = await _context.Rooms.Where(r => r.Status == "Available").ToListAsync();
                var empId2 = SessionHelper.GetUserID(HttpContext.Session);
                ViewBag.EmployeeID = await GetEmployeeIdForUser(empId2);
                return View(model);
            }

            bool unavailable = await _context.Reservations.AnyAsync(r =>
                r.RoomID == model.RoomID &&
                r.CheckInDate < model.CheckOutDate &&
                r.CheckOutDate > model.CheckInDate);

            if (unavailable)
            {
                ModelState.AddModelError("RoomID", "Room is not available for the selected dates.");
                ViewBag.Guests = await _context.Guests.OrderBy(g => g.LastName).ToListAsync();
                ViewBag.Rooms = await _context.Rooms.Where(r => r.Status == "Available").ToListAsync();
                var empId3 = SessionHelper.GetUserID(HttpContext.Session);
                ViewBag.EmployeeID = await GetEmployeeIdForUser(empId3);
                return View(model);
            }

            var room = await _context.Rooms.FindAsync(model.RoomID);
            if (room != null) room.Status = "Booked";

            _context.Reservations.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Reservation created successfully.";
            return RedirectToAction("GetReservationById", new { id = model.ReservationID });
        }

        public async Task<IActionResult> GetReservationById(int id)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var r = await _context.Reservations
                .Include(x => x.Guest)
                .Include(x => x.Room)
                .Include(x => x.Employee)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.ReservationID == id);

            if (r == null) return NotFound();

            var vm = new ReservationViewModel
            {
                ReservationID = r.ReservationID,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                GuestID = r.GuestID,
                GuestFullName = r.Guest.FullName,
                GuestPhone = r.Guest.Phone,
                GuestEmail = r.Guest.Email,
                RoomID = r.RoomID,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType,
                RoomPrice = r.Room.Price,
                EmployeeID = r.EmployeeID,
                EmployeeName = r.Employee.Name
            };
            ViewBag.Payments = r.Payments;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            var res = await _context.Reservations.Include(r => r.Room).FirstOrDefaultAsync(r => r.ReservationID == id);
            if (res != null)
            {
                if (res.Room != null) res.Room.Status = "Available";
                _context.Reservations.Remove(res);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reservation cancelled.";
            }
            return RedirectToAction("ListAllReservations");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            var res = await _context.Reservations.Include(r => r.Room).FirstOrDefaultAsync(r => r.ReservationID == id);
            if (res?.Room != null) res.Room.Status = "Booked";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Guest checked in successfully.";
            return RedirectToAction("ListAllReservations");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            var res = await _context.Reservations.Include(r => r.Room).FirstOrDefaultAsync(r => r.ReservationID == id);
            if (res?.Room != null) res.Room.Status = "Available";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Guest checked out successfully.";
            return RedirectToAction("ListAllReservations");
        }

        private async Task<int> GetEmployeeIdForUser(int? userID)
        {
            if (userID == null) return 1;
            var account = await _context.Accounts.FindAsync(userID);
            if (account?.EmployeeID == null)
            {
                var firstEmployee = await _context.Employees.FirstOrDefaultAsync();
                return firstEmployee?.EmployeeID ?? 1;
            }
            return account.EmployeeID.Value;
        }
    }
}
