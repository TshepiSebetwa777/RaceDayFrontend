using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace RaceDayFrontend.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient("RaceDayAPI");
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Account/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseData);
                var token = result.GetProperty("token").GetString();

                // Decode JWT to get user details
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type.EndsWith("role"))?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type.EndsWith("emailaddress"))?.Value;

                // Set session variables
                HttpContext.Session.SetString("JWToken", token);
                HttpContext.Session.SetString("UserRole", role ?? "Participant");
                HttpContext.Session.SetString("UserEmail", email ?? model.Email);

                if (role == "Organiser")
                    return RedirectToAction("Index", "Director");

                return RedirectToAction("Index", "Athlete");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient("RaceDayAPI");
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Account/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please sign in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Registration failed. Email might already be in use.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}