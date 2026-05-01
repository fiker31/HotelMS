using System.ComponentModel.DataAnnotations;

namespace HotelMS.Models.ViewModels
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Username is required"), MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required"), MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Staff";

        public int? EmployeeID { get; set; }
    }
}
