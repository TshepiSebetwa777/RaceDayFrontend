using System.ComponentModel.DataAnnotations;

namespace RaceDayFrontend.Models
{
    // The core event object coming from the API
    public class AthleteEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }
        public int Capacity { get; set; }
        public int CurrentRegistrations { get; set; }
        public decimal RegistrationFee { get; set; }
        public string CategoryName { get; set; } = string.Empty;
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