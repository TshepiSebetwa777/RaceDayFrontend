using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace RaceDayFrontend.Models
{
    // The core event object coming from the API
    public class AthleteEventDto
    {
        [JsonPropertyName("eventID")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("scheduledDate")]
        public DateTime RaceDate { get; set; }

        // Optional fields if the API sends them, mapped safely
        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("currentRegistrations")]
        public int CurrentRegistrations { get; set; }

        [JsonPropertyName("registrationFee")]
        public decimal RegistrationFee { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("organiser")]
        public string Organiser { get; set; } = string.Empty;
    }

    // The result object for the athlete's personal races
    public class AthleteResultDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int OverallPosition { get; set; }
    }

    // The profile object to handle Azure Blob Storage integration
    public class AthleteProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string? CurrentProfilePictureUrl { get; set; }

        [Display(Name = "Upload New Profile Picture")]
        public IFormFile? NewProfilePicture { get; set; }
    }
}