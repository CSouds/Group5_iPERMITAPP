// ============================================================
// RE (Regulated Entity) - Entity Class
// Holds all attributes, properties and operations that
// represent a Regulated Entity (RE).
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents a Regulated Entity (RE) - an Ontario business
    /// that requires Environmental permits for certain activities.
    /// </summary>
    public class RE
    {
        [Key]
        public string ID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required")]
        [Display(Name = "Contact Person Name")]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Account Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization name is required")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization address is required")]
        [Display(Name = "Organization Address")]
        public string OrganizationAddress { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<RESite> Sites { get; set; } = new List<RESite>();
        public virtual ICollection<PermitRequest> PermitRequests { get; set; } = new List<PermitRequest>();
    }
}
