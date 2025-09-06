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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var apiResponse = await _httpClient.GetAsync($"api/user/{userId}");
            if (!apiResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }
            var userProfile = await apiResponse.Content.ReadFromJsonAsync<UserProfileViewModel>();
            return View(userProfile);

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

            var user = await response.Content.ReadFromJsonAsync<Models.UserProfileViewModel>();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([FromForm] Models.UserProfileViewModel model)
        {

            model.Roles = new[] { "" }; // Agar roles ka default dena hai

            using var content = new MultipartFormDataContent();

            // Add string fields
            content.Add(new StringContent(model.UserName ?? ""), "UserName");
            content.Add(new StringContent(model.Email ?? ""), "Email");
            content.Add(new StringContent(model.Id.ToString()), "Id");
            content.Add(new StringContent(model.Roles[0].ToString()), "Roles");

            // Add file if present
            if (model.File != null && model.File.Length > 0)
            {
                var fileContent = new StreamContent(model.File.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.File.ContentType);
                content.Add(fileContent, "File", model.File.FileName);
            }

            var response = await _httpClient.PutAsync($"api/user/{model.Id}", content);
            Console.WriteLine(response.ToString);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Profile updated successfully.";
                return RedirectToAction("Index", "Dashboard");
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
        [Authorize(Roles = "Admin")]
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
