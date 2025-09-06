using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UMS.Web.Models;
using UMS.Web.Models.DTO_s;

namespace UMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API"); 
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var dto = new RegisterUserDTO
            {
                Name = model.UserName,
                Email = model.Email,
                Password = model.Password
            };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User registered successfully!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Registration failed. Try again.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
      
            var dto = new LoginRequestDTO
            {
                Email = model.Email,
                Password = model.Password
            };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Invalid email or password";
                return View(model);
            }
            var apiResult = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            if (apiResult == null || string.IsNullOrEmpty(apiResult.Token))
            {
                TempData["Error"] = "Login failed. Please try again.";
                return View(model);
            }

            Response.Cookies.Append("token", apiResult.Token, new CookieOptions
            {
                HttpOnly = true,     
                Secure = true,    
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1) 
            });

            return RedirectToAction("Index", "Dashboard");
        }

    }
}
