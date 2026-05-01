using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly HotelDbContext _context;
        public DashboardController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Login", "Account");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var today = DateTime.Today;
            var sixMonthsAgo = today.AddMonths(-6);

            var reservations = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.Employee)
                .Include(r => r.Payments)
                .ToListAsync();

            var payments = await _context.Payments.ToListAsync();

            var monthly = payments
                .Where(p => p.PaymentDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyRevenueData
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            var rooms = await _context.Rooms.ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalGuests = await _context.Guests.CountAsync(),
                TotalRooms = rooms.Count,
                AvailableRooms = rooms.Count(r => r.Status == "Available"),
                TotalReservations = reservations.Count,
                ActiveReservations = reservations.Count(r => today >= r.CheckInDate && today <= r.CheckOutDate),
                TotalRevenue = payments.Sum(p => p.Amount),
                TotalEmployees = await _context.Employees.CountAsync(),
                TodayCheckIns = reservations.Count(r => r.CheckInDate.Date == today),
                TodayCheckOuts = reservations.Count(r => r.CheckOutDate.Date == today),
                OccupancyRate = rooms.Count > 0
                    ? Math.Round((double)rooms.Count(r => r.Status == "Booked") / rooms.Count * 100, 1)
                    : 0,
                MonthlyRevenue = monthly,
                RoomTypeBreakdown = rooms.GroupBy(r => r.RoomType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentReservations = reservations
                    .OrderByDescending(r => r.ReservationID)
                    .Take(5)
                    .Select(r => new ReservationViewModel
                    {
                        ReservationID = r.ReservationID,
                        CheckInDate = r.CheckInDate,
                        CheckOutDate = r.CheckOutDate,
                        GuestID = r.GuestID,
                        GuestFullName = r.Guest.FullName,
                        RoomID = r.RoomID,
                        RoomNumber = r.Room.RoomNumber,
                        RoomType = r.Room.RoomType,
                        RoomPrice = r.Room.Price,
                        EmployeeID = r.EmployeeID,
                        EmployeeName = r.Employee.Name
                    }).ToList()
            };

            return View(vm);
        }
    }
}
