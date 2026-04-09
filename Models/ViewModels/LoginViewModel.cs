// ============================================================
// ViewModels - Used for form data binding in Views
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace Group5_iPERMITAPP.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Login form.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public string UserID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for the RE Registration form.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public string ID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required")]
        [Display(Name = "Contact Person Name")]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

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

        // Initial site information
        [Required(ErrorMessage = "At least one site address is required")]
        [Display(Name = "Site Address")]
        public string SiteAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Site contact person is required")]
        [Display(Name = "Site Contact Person")]
        public string SiteContactPerson { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for changing password.
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm new password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for the permit request form.
    /// </summary>
    public class PermitRequestViewModel
    {
        [Required(ErrorMessage = "Please select a permit type")]
        [Display(Name = "Environmental Permit Type")]
        public string EnvironmentalPermitID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activity description is required")]
        [Display(Name = "Activity Description")]
        public string ActivityDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activity start date is required")]
        [Display(Name = "Activity Start Date")]
        [DataType(DataType.Date)]
        public DateTime ActivityStartDate { get; set; }

        [Required(ErrorMessage = "Activity duration is required")]
        [Display(Name = "Activity Duration (days)")]
        [Range(1, 3650, ErrorMessage = "Duration must be between 1 and 3650 days")]
        public int ActivityDuration { get; set; }

        [Display(Name = "Site")]
        public int? SiteID { get; set; }
    }

    /// <summary>
    /// ViewModel for processing payment through OPS-CPP.
    /// </summary>
    public class PaymentViewModel
    {
        public string PermitRequestNo { get; set; } = string.Empty;
        public double Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card holder name is required")]
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [MinLength(16, ErrorMessage = "Card number must be 16 digits")]
        [MaxLength(16, ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required")]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [MinLength(3, ErrorMessage = "CVV must be 3 digits")]
        [MaxLength(3, ErrorMessage = "CVV must be 3 digits")]
        public string CVV { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for the EO review decision.
    /// </summary>
    public class ReviewDecisionViewModel
    {
        public string PermitRequestNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Decision is required")]
        [Display(Name = "Decision")]
        public string FinalDecision { get; set; } = string.Empty; // "Approved" or "Rejected"

        [Display(Name = "Reason / Description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for the RE dashboard.
    /// </summary>
    public class REDashboardViewModel
    {
        public RE RE { get; set; } = null!;
        public List<PermitRequest> PermitRequests { get; set; } = new();
        public Dictionary<string, string> CurrentStatuses { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for EO dashboard.
    /// </summary>
    public class EODashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int PendingReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int PermitsIssued { get; set; }
        public List<PermitRequest> RecentApplications { get; set; } = new();
    }
}
