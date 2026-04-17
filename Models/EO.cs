// ============================================================
// EO (Environmental Officer) - Entity Class
// Represents the Ministry's Environmental Officer who
// reviews and decides on permit requests.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents the Ministry's Environmental Officer (EO)
    /// who reviews RE applications and issues permits.
    /// </summary>
    public class EO
    {
        [Key]
        public string ID { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Officer Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
