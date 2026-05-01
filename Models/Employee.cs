using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelMS.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = "Staff";

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Salary { get; set; }

        [Required, MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public Account? Account { get; set; }
    }
}
