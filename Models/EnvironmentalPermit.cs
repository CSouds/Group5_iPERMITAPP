// ============================================================
// Environmental Permits - Entity Class
// Maintains basic information on the available permits
// and their tariff.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents a type of environmental permit available
    /// in the system, with its associated fee.
    /// </summary>
    public class EnvironmentalPermit
    {
        [Key]
        public string PermitID { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Permit Name")]
        public string PermitName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Permit Fee ($)")]
        [DataType(DataType.Currency)]
        public double PermitFee { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
    }
}
