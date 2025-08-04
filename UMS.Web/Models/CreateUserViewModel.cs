using System.ComponentModel.DataAnnotations;

namespace UMS.Web.Models
{
    public class CreateUserViewModel
    {
        [Required, Display(Name = "User Name")]
        public string Name { get; set; }

        [Required, EmailAddress, Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Display(Name = "Role")]
        public string Role { get; set; }
    }
}
