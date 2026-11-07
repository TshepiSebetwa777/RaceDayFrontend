using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace RaceDayFrontend.Controllers
{
    public class DirectorController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public DirectorController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Middleware check to ensure ONLY Race Directors (Organisers) can access this
        private HttpClient GetAuthenticatedDirectorClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Organiser") return null!;

            var client = _clientFactory.CreateClient("RaceDayAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // 1. Analytics Dashboard
        public async Task<IActionResult> Index()
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Events");
            var events = new List<AthleteEventDto>(); // Reusing the event DTO to count stats

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                events = JsonSerializer.Deserialize<List<AthleteEventDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteEventDto>();
            }

            // Calculate live analytics for the dashboard
            var dashboardData = new DirectorDashboardViewModel
            {
                TotalEventsManaged = events.Count,
                TotalActiveAthletes = events.Sum(e => e.CurrentRegistrations),
                TotalRevenueGenerated = events.Sum(e => e.CurrentRegistrations * e.RegistrationFee)
            };

            return View(dashboardData);
        }

        // 2. Create Event (GET)
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (GetAuthenticatedDirectorClient() == null) return RedirectToAction("Login", "Account");

            // Default date to next month
            return View(new CreateEventViewModel { RaceDate = DateTime.Now.AddDays(30) });
        }

        // 3. Create Event (POST)
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("Events", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "New race event published successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not create the event. Please verify category ID exists.");
            }

            return View(model);
        }
    }
}