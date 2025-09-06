using System.ComponentModel.DataAnnotations;

namespace UMS.Web.Models
{
    public class UserProfileViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string[] Roles { get; set; }

        public IFormFile? File { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
