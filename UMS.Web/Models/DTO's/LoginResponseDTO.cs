using System.Net;

namespace UMS.Web.Models.DTO_s
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
