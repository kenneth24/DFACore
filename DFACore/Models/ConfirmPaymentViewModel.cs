using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class ConfirmPaymentViewModel
    {
        public string PaymentToken { get; set; }

        public string PaymentRequestId { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        [RegularExpression("^[0-9]+$", MatchTimeoutInMilliseconds = 3000, ErrorMessage = "Invalid otp.")]
        public string Otp { get; set; }
    }
}
