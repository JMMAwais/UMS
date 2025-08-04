using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UMS.Web.Models
{
    public class RegisterUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, and 1 special character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [JsonIgnore]
        public string ConfirmPassword { get; set; }
        public string? Role { get; set; } = "User"; 
    }
}
