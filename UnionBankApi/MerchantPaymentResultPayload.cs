using System;

namespace UnionBankApi
{
    public class MerchantPaymentResultPayload
    {
        public string Code { get; set; }

        public string SenderRefId { get; set; }

        public string State { get; set; }

        public string Uuid { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public string Amount { get; set; }

        public string UbpTranId { get; set; }

        public DateTime TranRequestDate { get; set; }
    }
}
