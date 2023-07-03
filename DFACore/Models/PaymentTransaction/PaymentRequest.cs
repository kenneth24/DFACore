using System.Collections.Generic;

namespace DFACore.Models.PaymentTransaction
{
    public class PaymentRequest
    {
        public string applicationCode { get; set; }
        public string amount { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
        public string co_selected { get; set; }
        public List<PaymentDetail> details { get; set; }
    }
}
