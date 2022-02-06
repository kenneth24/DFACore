namespace UnionBankApi
{
    public class MerchantPaymentResult
    {
        public MerchantPaymentResultPayload Payload { get; set; }

        public string Signature { get; set; }
    }
}
