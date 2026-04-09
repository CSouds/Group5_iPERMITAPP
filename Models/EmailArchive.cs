// ============================================================
// EmailArchive - Entity Class
// Logs all emails sent from the EO and OPS-CPP to the RE
// for future reference.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Archives all email communications sent to the RE
    /// throughout the permit request process.
    /// </summary>
    public class EmailArchive
    {
        [Key]
        public string EmailID { get; set; } = string.Empty;

        [Display(Name = "Email Date")]
        [DataType(DataType.Date)]
        public DateTime EmailDate { get; set; } = DateTime.Now;

        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Sent By")]
        public string SentBy { get; set; } = string.Empty;

        [Display(Name = "Sent To")]
        public string SentTo { get; set; } = string.Empty;

        [Display(Name = "Recipient Email")]
        public string RecipientEmail { get; set; } = string.Empty;

        // Related permit request number
        public string? PermitRequestNo { get; set; }
    }
}
