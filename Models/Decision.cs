// ============================================================
// Decision - Entity Class
// Records the EO's decision to accept or reject a permit
// request, along with the reasoning.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Records the EO's decision on a permit request
    /// (either "Approved" or "Rejected").
    /// </summary>
    public class Decision
    {
        [Key]
        public string ID { get; set; } = string.Empty;

        [Display(Name = "Decision Date")]
        [DataType(DataType.Date)]
        public DateTime DateOfDecision { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Final Decision")]
        public string FinalDecision { get; set; } = string.Empty; // "Approved" or "Rejected"

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // Foreign key to EO (madeBy)
        [Required]
        public string EOID { get; set; } = string.Empty;

        [ForeignKey("EOID")]
        public virtual EO? MadeBy { get; set; }

        // Foreign key to PermitRequest (relatedTo)
        [Required]
        public string PermitRequestNo { get; set; } = string.Empty;

        [ForeignKey("PermitRequestNo")]
        public virtual PermitRequest? RelatedTo { get; set; }
    }
}
