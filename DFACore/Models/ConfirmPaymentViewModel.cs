using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class ConfirmPaymentViewModel
    {
        //Customer Account Access Token
        public string Caat { get; set; }

        public string OtpRequestId { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        [RegularExpression("^[0-9]+$", MatchTimeoutInMilliseconds = 3000, ErrorMessage = "Invalid otp.")]
        public string Otp { get; set; }
    }
}
