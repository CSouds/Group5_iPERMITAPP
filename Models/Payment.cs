// ============================================================
// Payment - Entity Class
// Maintains payment transaction records including date,
// method, and approval status.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents a payment transaction for a permit request,
    /// processed through the OPS Common Payment Portal (CPP).
    /// </summary>
    public class Payment
    {
        [Key]
        public string PaymentID { get; set; } = string.Empty;

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty; // Visa, MasterCard, PayPal, etc.

        [Display(Name = "Last 4 Digits of Card")]
        public string Last4DigitOfCard { get; set; } = string.Empty;

        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; } = string.Empty;

        [Display(Name = "Payment Approved")]
        public bool PaymentApproved { get; set; } = false;

        // Foreign key to PermitRequest
        [Required]
        public string PermitRequestNo { get; set; } = string.Empty;

        [ForeignKey("PermitRequestNo")]
        public virtual PermitRequest? PermitRequest { get; set; }
    }
}
