using Microsoft.AspNetCore.Http;

namespace UMS.Web.Models.Dashboard
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public IFormFile? ProfileIamge { get; set; }
    }
}
