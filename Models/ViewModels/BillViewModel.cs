namespace HotelMS.Models.ViewModels
{
    public class BillViewModel
    {
        public int ReservationID { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalNights => Math.Max((CheckOutDate - CheckInDate).Days, 0);
        public decimal PricePerNight { get; set; }
        public decimal SubTotal => TotalNights * PricePerNight;
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalPaid => Payments.Sum(p => p.Amount);
        public decimal BalanceDue => SubTotal - TotalPaid;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string HandledBy { get; set; } = string.Empty;
    }
}
