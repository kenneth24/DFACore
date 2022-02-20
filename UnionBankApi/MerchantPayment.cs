using System;
using System.Collections.Generic;

namespace UnionBankApi
{
    public class MerchantPayment
    {
        public string SenderRefId { get; set; }

        public DateTime TranRequestDate { get; set; }

        public string Remarks { get; set; }

        public string Particulars { get; set; }

        public PaymentAmount Amount { get; set; }

        public List<PaymentInformation> Info { get; set; }

        public string RequestId { get; set; }

        public string Otp { get; set; }

        public MerchantPayment()
        {
            Remarks = "none";
            Particulars = "none";
            Info = new List<PaymentInformation>();
        }
    }
}
