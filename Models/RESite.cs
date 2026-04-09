// ============================================================
// RE Site - Entity Class
// Each RE's organization has one or more sites to which
// the permit is requested.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5_iPERMITAPP.Models
{
    /// <summary>
    /// Represents a site belonging to a Regulated Entity (RE)
    /// where environmental activities occur.
    /// </summary>
    public class RESite
    {
        [Key]
        public int SiteID { get; set; }

        [Required(ErrorMessage = "Site address is required")]
        [Display(Name = "Site Address")]
        public string SiteAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Site contact person is required")]
        [Display(Name = "Site Contact Person")]
        public string SiteContactPerson { get; set; } = string.Empty;

        // Foreign key to RE
        [Required]
        public string REID { get; set; } = string.Empty;

        [ForeignKey("REID")]
        public virtual RE? RE { get; set; }
    }
}
