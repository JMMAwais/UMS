using System.ComponentModel.DataAnnotations;

namespace UMS.Web.Models.Dashboard
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        [Required, Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required, EmailAddress, Display(Name = "Email Address")]
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}
