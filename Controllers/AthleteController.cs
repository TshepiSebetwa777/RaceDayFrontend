using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDayFrontend.Controllers
{
    public class AthleteController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AthleteController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Middleware check to ensure only logged-in athletes can access this
        private HttpClient GetAuthenticatedClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return null!;

            var client = _clientFactory.CreateClient("RaceDayAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // 1. Browse All Upcoming Races
        public async Task<IActionResult> Index()
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Events");
            var events = new List<AthleteEventDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                events = JsonSerializer.Deserialize<List<AthleteEventDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteEventDto>();
            }

            return View(events);
        }

        // 2. Enrol in a Race
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.PostAsync($"Enrolments/{eventId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully registered for the race!";
            }
            else
            {
                TempData["ErrorMessage"] = "Registration failed. The race might be full or you are already enrolled.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. View Personal Results
        public async Task<IActionResult> MyResults()
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            // We assume the API has an endpoint to get results for the logged-in user
            var response = await client.GetAsync("Results/myresults");
            var results = new List<AthleteResultDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                results = JsonSerializer.Deserialize<List<AthleteResultDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteResultDto>();
            }

            return View(results);
        }

        // 4. Update Profile (Azure Blob Storage Integration - Stubbed for the view)
        [HttpGet]
        public IActionResult Profile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return RedirectToAction("Login", "Account");

            var model = new AthleteProfileViewModel
            {
                FirstName = "Athlete", // Ideally fetched from API
                LastName = "User"
            };
            return View(model);
        }
    }
}