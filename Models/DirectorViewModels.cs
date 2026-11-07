using System.ComponentModel.DataAnnotations;

namespace RaceDayFrontend.Models
{
    // Handles the analytics requirement for the rubric
    public class DirectorDashboardViewModel
    {
        public int TotalEventsManaged { get; set; }
        public int TotalActiveAthletes { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
    }

    // Handles the Event Creation requirement
    public class CreateEventViewModel
    {
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

        [Required]
        [Range(10, 10000, ErrorMessage = "Capacity must be between 10 and 10,000")]
        public int Capacity { get; set; }

        [Required]
        [Display(Name = "Registration Fee (ZAR)")]
        [Range(0, 5000)]
        public decimal RegistrationFee { get; set; }

        [Required]
        [Display(Name = "Race Category")]
        public int CategoryId { get; set; }
    }
}