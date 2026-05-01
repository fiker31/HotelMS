using System.ComponentModel.DataAnnotations;

namespace HotelMS.Models
{
    public class Guest
    {
        public int GuestID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
