using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace RaceDayFrontend.Models
{
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

        [JsonPropertyName("organiser")]
        public string Organiser { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }

    public class AthleteEnrolmentDto
    {
        [JsonPropertyName("enrolmentId")]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("eventTitle")]
        public string EventTitle { get; set; } = string.Empty;

        [JsonPropertyName("raceDate")]
        public DateTime RaceDate { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("distanceKm")]
        public decimal DistanceKm { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class AthleteResultDto
    {
        [JsonPropertyName("eventTitle")]
        public string EventTitle { get; set; } = string.Empty;

        [JsonPropertyName("raceDate")]
        public DateTime RaceDate { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("timeRecorded")]
        public string TimeRecorded { get; set; } = string.Empty;

        [JsonPropertyName("medalType")]
        public string MedalType { get; set; } = string.Empty;
    }

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