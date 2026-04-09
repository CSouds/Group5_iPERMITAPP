// ============================================================
// Permit - Entity Class
// Maintains all information of the approved issued permits.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents an issued environmental permit that was
    /// approved by the EO.
    /// </summary>
    public class Permit
    {
        [Key]
        public string PermitID { get; set; } = string.Empty;

        [Display(Name = "Date of Issue")]
        [DataType(DataType.Date)]
        public DateTime DateOfIssue { get; set; } = DateTime.Now;

        [Display(Name = "Duration")]
        public string Duration { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // Foreign key to PermitRequest (relatedTo)
        [Required]
        public string PermitRequestNo { get; set; } = string.Empty;

        [ForeignKey("PermitRequestNo")]
        public virtual PermitRequest? RelatedTo { get; set; }

        // Foreign key to EO (issuedBy)
        [Required]
        public string EOID { get; set; } = string.Empty;

        [ForeignKey("EOID")]
        public virtual EO? IssuedBy { get; set; }

        // Foreign key to RE (issuedTo)
        [Required]
        public string REID { get; set; } = string.Empty;

        [ForeignKey("REID")]
        public virtual RE? IssuedTo { get; set; }
    }
}
