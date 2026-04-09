// ============================================================
// PermitRequest - Entity Class
// Holds the attributes, properties and operations related
// to the permit request transactions.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents a permit request submitted by an RE.
    /// Tracks the full lifecycle from submission to issuance.
    /// </summary>
    public class PermitRequest
    {
        [Key]
        public string RequestNo { get; set; } = string.Empty;

        [Display(Name = "Date of Request")]
        [DataType(DataType.Date)]
        public DateTime DateOfRequest { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Activity description is required")]
        [Display(Name = "Activity Description")]
        public string ActivityDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activity start date is required")]
        [Display(Name = "Activity Start Date")]
        [DataType(DataType.Date)]
        public DateTime ActivityStartDate { get; set; }

        [Required(ErrorMessage = "Activity duration is required")]
        [Display(Name = "Activity Duration (days)")]
        public int ActivityDuration { get; set; }

        [Display(Name = "Permit Fee ($)")]
        [DataType(DataType.Currency)]
        public double PermitFee { get; set; }

        // Foreign key to RE (requestedBy)
        [Required]
        public string REID { get; set; } = string.Empty;

        [ForeignKey("REID")]
        public virtual RE? RequestedBy { get; set; }

        // Foreign key to EnvironmentalPermit (requestedPermit)
        [Required(ErrorMessage = "Please select a permit type")]
        [Display(Name = "Permit Type")]
        public string EnvironmentalPermitID { get; set; } = string.Empty;

        [ForeignKey("EnvironmentalPermitID")]
        public virtual EnvironmentalPermit? RequestedPermit { get; set; }

        // Navigation properties
        public virtual ICollection<RequestStatus> Statuses { get; set; } = new List<RequestStatus>();
        public virtual Payment? Payment { get; set; }
        public virtual Decision? Decision { get; set; }
        public virtual Permit? IssuedPermit { get; set; }
    }
}
