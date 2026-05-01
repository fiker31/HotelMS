using System.ComponentModel.DataAnnotations;

namespace HotelMS.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int GuestID { get; set; }

        [Required]
        public int RoomID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        public Guest Guest { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public int TotalNights => (CheckOutDate - CheckInDate).Days;
    }
}
