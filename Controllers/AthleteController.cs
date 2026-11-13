using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using RaceDayFrontend.Services;
using System.Text.Json;
using System.Text;

namespace RaceDayFrontend.Controllers
{
    public class AthleteController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AzureBlobService _blobService;

        public AthleteController(IHttpClientFactory clientFactory, AzureBlobService blobService)
        {
            _clientFactory = clientFactory;
            _blobService = blobService;
        }

        // BULLETPROOF COOKIE BRIDGE
        private HttpClient GetAuthenticatedClient()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role)) return null!;

            var client = _clientFactory.CreateClient("RaceDayAPI");

            var apiCookie = HttpContext.Session.GetString("ApiCookie");
            if (!string.IsNullOrEmpty(apiCookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", apiCookie);
            }
            else
            {
                // Fallback: If dropped, grab the browser's active session cookie
                if (Request.Cookies.TryGetValue(".AspNetCore.Session", out var browserCookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Session={browserCookie}");
                }
            }

            return client;
        }

        public async Task<IActionResult> Index(string searchString)
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

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e =>
                    e.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    e.Organiser.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            events = events.OrderBy(e => e.RaceDate).ToList();
            ViewData["CurrentFilter"] = searchString;
            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync($"Events/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var eventDetail = JsonSerializer.Deserialize<AthleteEventDto>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(eventDetail);
            }

            TempData["ErrorMessage"] = "Event details not found.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var payload = new { CategoryId = categoryId };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Enrolments", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled for the race!";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Enrolment failed. {errorMsg}";
            }

            return RedirectToAction(nameof(MyEvents));
        }

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Enrolments/myenrolments");
            var myEvents = new List<AthleteEnrolmentDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                myEvents = JsonSerializer.Deserialize<List<AthleteEnrolmentDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteEnrolmentDto>();
            }

            return View(myEvents);
        }

        public async Task<IActionResult> MyResults()
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Results/myresults");
            var results = new List<AthleteResultDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                results = JsonSerializer.Deserialize<List<AthleteResultDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteResultDto>();
            }
            return View(results);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var model = new AthleteProfileViewModel
            {
                FirstName = "Athlete",
                LastName = "User",
                CurrentProfilePictureUrl = TempData["ProfilePicUrl"]?.ToString()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(AthleteProfileViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            if (model.NewProfilePicture != null)
            {
                try
                {
                    var imageUrl = await _blobService.UploadEventBannerAsync(model.NewProfilePicture, email);

                    var payload = new { FullName = $"{model.FirstName} {model.LastName}", ProfilePictureUrl = imageUrl };
                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var client = GetAuthenticatedClient();
                    await client.PutAsync("Account/profile", content);

                    TempData["ProfilePicUrl"] = imageUrl;
                    TempData["SuccessMessage"] = "Profile picture updated successfully in Azure Blob Storage!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Failed to upload image: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}