using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using RaceDayFrontend.Services;
using System.Net.Http.Headers;
using System.Text.Json;

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

        private HttpClient GetAuthenticatedClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return null!;

            var client = _clientFactory.CreateClient("RaceDayAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

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

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            var client = GetAuthenticatedClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.PostAsync($"Enrolments/{eventId}", null);
            if (response.IsSuccessStatusCode) TempData["SuccessMessage"] = "Successfully registered for the race!";
            else TempData["ErrorMessage"] = "Registration failed. The race might be full or you are already enrolled.";

            return RedirectToAction(nameof(Index));
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

            // In a real app, we'd fetch current details from the API. We'll simulate it.
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
                    // Call our new dedicated Azure Blob Service
                    var imageUrl = await _blobService.UploadProfilePictureAsync(model.NewProfilePicture, email);
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