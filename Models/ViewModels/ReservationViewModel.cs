namespace HotelMS.Models.ViewModels
{
    public class ReservationViewModel
    {
        public int ReservationID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GuestID { get; set; }
        public string GuestFullName { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal RoomPrice { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalNights => Math.Max((CheckOutDate - CheckInDate).Days, 0);
        public decimal TotalAmount => TotalNights * RoomPrice;

        public string Status
        {
            get
            {
                var today = DateTime.Today;
                if (today < CheckInDate) return "Upcoming";
                if (today <= CheckOutDate) return "Active";
                return "Completed";
            }
        }
    }
}
