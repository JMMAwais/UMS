using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using UMS.Web.Models;
using UMS.Web.Models.Dashboard;
using UMS.Web.Models.DTO_s;

namespace UMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public IActionResult Index()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                Response.Cookies.Delete("token");
                return RedirectToAction("Login", "Account");
            }

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value;

            ViewBag.UserId = userId;
            ViewBag.Email = email;
            ViewBag.Role = role;

            return View();

        }

        public async Task<IActionResult> GetAllUser()
        {
            var response = await _httpClient.GetAsync("/api/user/AllUsers");
            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized();
            }
            var users = await response.Content.ReadFromJsonAsync<List<UserListViewModel>>();
            return View("AllUser", users ?? new List<UserListViewModel>());
        }

        public async Task<IActionResult> Edit(string id)
        {

            if (string.IsNullOrEmpty(id)) return NotFound();

            var response = await _httpClient.GetAsync($"/api/user/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var apiUser = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (apiUser == null) return NotFound();

            var model = new UserListViewModel
            {
                Id = apiUser.Id,
                UserName = apiUser.UserName,
                Email = apiUser.Email,
                Roles = apiUser.Roles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserListViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PutAsJsonAsync($"/api/user/{model.Id}", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(GetAllUser));
            }

            ModelState.AddModelError("", "Failed to update user.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var response = await _httpClient.DeleteAsync($"/api/user/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "User deleted successfully.";
                return RedirectToAction(nameof(GetAllUser));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                TempData["Error"] = "Only Admin can delete users.";
                return RedirectToAction(nameof(GetAllUser));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to delete user: {error}";
                return RedirectToAction(nameof(GetAllUser));
            }
        }

        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _httpClient.GetAsync($"/api/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var user = await response.Content.ReadFromJsonAsync<UserProfileViewModel>();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel model)
        {
            
            model.Roles = [""];
            //if (!ModelState.IsValid)
            //    return View(model);
            var response = await _httpClient.PutAsJsonAsync($"api/user/{model.Id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Profile updated successfully.";
                return RedirectToAction("Dashboard", "Index");
            }

            TempData["Error"] = "Failed to update profile.";
            return View(model);
        }


        public IActionResult Logout()
        {
            Response.Cookies.Delete("YourJwtCookieName");
            return RedirectToAction("Login", "Account");
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "User created successfully.";
                return RedirectToAction("GetAllUser");
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Failed to create user: {error}";
            return View(model);
        }


    }
}
