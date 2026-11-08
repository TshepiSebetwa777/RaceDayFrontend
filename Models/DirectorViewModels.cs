using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace RaceDayFrontend.Models
{
    public class DirectorDashboardViewModel
    {
        public int TotalEventsManaged { get; set; }
        public int TotalActiveAthletes { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
    }

    public class CategoryDto
    {
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("eventId")]
        public int EventId { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;

        [JsonPropertyName("distanceKm")]
        public decimal DistanceKM { get; set; }
    }

    public class CreateEventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Race title is required")]
        [Display(Name = "Race Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Scheduled Date")]
        [DataType(DataType.Date)]
        public DateTime RaceDate { get; set; } = DateTime.Now.AddDays(30);

        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "Event Banner Image")]
        public IFormFile? BannerImage { get; set; }

        public string? BannerUrl { get; set; }
    }

    public class EditEventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Race title is required")]
        [Display(Name = "Race Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Scheduled Date")]
        [DataType(DataType.Date)]
        public DateTime RaceDate { get; set; }

        [Display(Name = "Update Event Banner (Azure)")]
        public IFormFile? BannerImage { get; set; }
    }

    public class EnrolmentListDto
    {
        public int Id { get; set; }
        public string AthleteName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CaptureResultViewModel
    {
        [Required]
        public int EnrolmentId { get; set; }

        public string? AthleteName { get; set; }
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Please enter the finish time")]
        [Display(Name = "Finish Time (HH:MM:SS)")]
        [RegularExpression(@"^\d{2}:\d{2}:\d{2}$", ErrorMessage = "Format must be HH:MM:SS (e.g., 02:15:30)")]
        public string TimeRecorded { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a position or medal")]
        [Display(Name = "Finishing Position / Medal")]
        public string MedalType { get; set; } = string.Empty;
    }
}