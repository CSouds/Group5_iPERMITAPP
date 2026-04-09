// ============================================================
// RequestStatus - Entity Class
// Maintains the different statuses of an RE's permit request
// throughout its lifecycle.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Tracks the status history of a permit request.
    /// Statuses: Pending Payment, Submitted, Being Reviewed,
    /// Approved, Rejected, Permit Issued.
    /// </summary>
    public class RequestStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string PermitRequestStatus { get; set; } = string.Empty;

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // Foreign key to PermitRequest
        [Required]
        public string PermitRequestNo { get; set; } = string.Empty;

        [ForeignKey("PermitRequestNo")]
        public virtual PermitRequest? PermitRequest { get; set; }

        // Who updated the status (updatedBy)
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
