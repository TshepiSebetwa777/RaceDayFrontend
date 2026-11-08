using Microsoft.AspNetCore.Mvc;
using RaceDayFrontend.Models;
using RaceDayFrontend.Services;
using System.Text.Json;
using System.Text;

namespace RaceDayFrontend.Controllers
{
    public class DirectorController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AzureBlobService _blobService;

        public DirectorController(IHttpClientFactory clientFactory, AzureBlobService blobService)
        {
            _clientFactory = clientFactory;
            _blobService = blobService;
        }

        private HttpClient GetAuthenticatedDirectorClient()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser") return null!;

            return _clientFactory.CreateClient("RaceDayAPI");
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Events");
            var events = new List<AthleteEventDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                events = JsonSerializer.Deserialize<List<AthleteEventDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AthleteEventDto>();
            }

            var dashboardData = new DirectorDashboardViewModel
            {
                TotalEventsManaged = events.Count,
                TotalActiveAthletes = events.Sum(e => e.CurrentRegistrations),
                TotalRevenueGenerated = events.Sum(e => e.CurrentRegistrations * e.RegistrationFee)
            };

            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            if (GetAuthenticatedDirectorClient() == null) return RedirectToAction("Login", "Account");

            var model = new CreateEventViewModel();

            var client = GetAuthenticatedDirectorClient();
            var response = await client.GetAsync("Categories");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryDto>();
                ViewBag.EventTypes = categories.Select(c => c.TypeName).Distinct().ToList();
            }
            else
            {
                ViewBag.EventTypes = new List<string> { "Road Race", "Trail Run", "Track", "Virtual" };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                string uploadedBannerUrl = string.Empty;
                if (model.BannerImage != null)
                {
                    uploadedBannerUrl = await _blobService.UploadEventBannerAsync(model.BannerImage, model.Title);
                }

                string enrichedDescription = $"{model.Description} | Location: {model.Location} | Distance: {model.Distance}km | Type: {model.EventType}";

                var apiPayload = new
                {
                    Title = model.Title,
                    ScheduledDate = model.RaceDate,
                    Description = enrichedDescription,
                    BannerUrl = uploadedBannerUrl
                };

                var content = new StringContent(JsonSerializer.Serialize(apiPayload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("Events", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "New race event published successfully!";
                    return RedirectToAction(nameof(ManageEvents));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not create the event. Verify backend connection.");
            }

            var categoryResponse = await client.GetAsync("Categories");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var json = await categoryResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryDto>();
                ViewBag.EventTypes = categories.Select(c => c.TypeName).Distinct().ToList();
            }
            else
            {
                ViewBag.EventTypes = new List<string> { "Road Race", "Trail Run", "Track", "Virtual" };
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageEvents()
        {
            var client = GetAuthenticatedDirectorClient();
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
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.DeleteAsync($"Events/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Event successfully deleted.";
            else
                TempData["ErrorMessage"] = "Failed to delete event. Ensure there are no active enrolments.";

            return RedirectToAction(nameof(ManageEvents));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Events");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<AthleteEventDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var eventToEdit = events?.FirstOrDefault(e => e.Id == id);

                if (eventToEdit != null)
                {
                    var model = new EditEventViewModel
                    {
                        EventId = eventToEdit.Id,
                        Title = eventToEdit.Title,
                        RaceDate = eventToEdit.RaceDate,
                        Description = eventToEdit.Description
                    };
                    return View(model);
                }
            }
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(ManageEvents));
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(EditEventViewModel model)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var apiPayload = new
                {
                    Title = model.Title,
                    ScheduledDate = model.RaceDate,
                    Description = model.Description
                };

                var content = new StringContent(JsonSerializer.Serialize(apiPayload), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"Events/{model.EventId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event updated successfully!";
                    return RedirectToAction(nameof(ManageEvents));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not update the event.");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId, string eventTitle)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync("Categories");
            var allCategories = new List<CategoryDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                allCategories = JsonSerializer.Deserialize<List<CategoryDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryDto>();
            }

            var eventCategories = allCategories.Where(c => c.EventId == eventId).ToList();

            ViewBag.EventId = eventId;
            ViewBag.EventTitle = eventTitle;
            return View(eventCategories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string eventTitle, string typeName, decimal distanceKm)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var payload = new
            {
                EventId = eventId,
                TypeName = typeName,
                DistanceKm = distanceKm
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("Categories", content);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Category added successfully!";
            else
                TempData["ErrorMessage"] = "Failed to add category.";

            return RedirectToAction(nameof(ManageCategories), new { eventId = eventId, eventTitle = eventTitle });
        }

        [HttpGet]
        public async Task<IActionResult> Enrolments(int id, string title)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var response = await client.GetAsync($"Events/{id}/enrolments");
            var enrolments = new List<EnrolmentListDto>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                enrolments = JsonSerializer.Deserialize<List<EnrolmentListDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EnrolmentListDto>();
            }

            ViewBag.EventTitle = title;
            return View(enrolments);
        }

        [HttpGet]
        public IActionResult CaptureResult(int enrolmentId, string athleteName, string categoryName)
        {
            if (GetAuthenticatedDirectorClient() == null) return RedirectToAction("Login", "Account");

            var model = new CaptureResultViewModel
            {
                EnrolmentId = enrolmentId,
                AthleteName = athleteName,
                CategoryName = categoryName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(CaptureResultViewModel model)
        {
            var client = GetAuthenticatedDirectorClient();
            if (client == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var payload = new
                {
                    EnrolmentId = model.EnrolmentId,
                    TimeRecorded = model.TimeRecorded,
                    MedalType = model.MedalType
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("Results", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Result successfully published for {model.AthleteName}!";
                    return RedirectToAction(nameof(ManageEvents));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not capture result. It may already exist.");
            }

            return View(model);
        }
    }
}