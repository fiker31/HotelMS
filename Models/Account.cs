using System.ComponentModel.DataAnnotations;

namespace HotelMS.Models
{
    public class Account
    {
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Staff";

        public int? EmployeeID { get; set; }

        public Employee? Employee { get; set; }
    }
}
