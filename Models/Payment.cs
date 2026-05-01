using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelMS.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        public int ReservationID { get; set; }

        public Reservation Reservation { get; set; } = null!;
    }
}
