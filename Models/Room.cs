using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelMS.Models
{
    public class Room
    {
        public int RoomID { get; set; }

        [Required, MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = "Single";

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
