using HotelMS.Data;
using HotelMS.Helpers;
using HotelMS.Models;
using HotelMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HotelDbContext _context;
        public PaymentController(HotelDbContext context) => _context = context;

        private IActionResult? CheckAuth(params string[] roles)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Login", "Account");
            if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
                return RedirectToAction("Index", "Dashboard");
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int reservationId)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            var res = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

            if (res == null) return NotFound();

            var totalDue = res.TotalNights * res.Room.Price;
            var totalPaid = res.Payments.Sum(p => p.Amount);

            ViewBag.Reservation = res;
            ViewBag.TotalDue = totalDue;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.BalanceDue = totalDue - totalPaid;

            return View(new Payment { ReservationID = reservationId, Amount = totalDue - totalPaid });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(Payment model)
        {
            var auth = CheckAuth("Admin", "Manager", "Receptionist");
            if (auth != null) return auth;

            if (model.Amount <= 0) ModelState.AddModelError("Amount", "Amount must be greater than zero.");
            if (!ModelState.IsValid)
            {
                var res2 = await _context.Reservations
                    .Include(r => r.Guest).Include(r => r.Room).Include(r => r.Payments)
                    .FirstOrDefaultAsync(r => r.ReservationID == model.ReservationID);
                ViewBag.Reservation = res2;
                return View(model);
            }

            model.PaymentDate = DateTime.Now;
            _context.Payments.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Payment of ${model.Amount:N2} recorded.";
            return RedirectToAction("GenerateBill", new { reservationId = model.ReservationID });
        }

        public async Task<IActionResult> GenerateBill(int reservationId)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var res = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.Employee)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

            if (res == null) return NotFound();

            var vm = new BillViewModel
            {
                ReservationID = res.ReservationID,
                GuestName = res.Guest.FullName,
                GuestPhone = res.Guest.Phone,
                GuestEmail = res.Guest.Email,
                RoomNumber = res.Room.RoomNumber,
                RoomType = res.Room.RoomType,
                CheckInDate = res.CheckInDate,
                CheckOutDate = res.CheckOutDate,
                PricePerNight = res.Room.Price,
                Payments = res.Payments.ToList(),
                HandledBy = res.Employee.Name
            };
            return View(vm);
        }

        public async Task<IActionResult> GetPaymentById(int id)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var payment = await _context.Payments
                .Include(p => p.Reservation).ThenInclude(r => r.Guest)
                .Include(p => p.Reservation).ThenInclude(r => r.Room)
                .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null) return NotFound();
            return View(payment);
        }

        public async Task<IActionResult> ListAllPayments(DateTime? from, DateTime? to)
        {
            var auth = CheckAuth();
            if (auth != null) return auth;

            var query = _context.Payments
                .Include(p => p.Reservation).ThenInclude(r => r.Guest)
                .Include(p => p.Reservation).ThenInclude(r => r.Room)
                .AsQueryable();

            if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value.AddDays(1));

            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            var list = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
            ViewBag.TotalRevenue = list.Sum(p => p.Amount);
            return View(list);
        }
    }
}
