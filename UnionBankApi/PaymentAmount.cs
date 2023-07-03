namespace UnionBankApi
{
    public class PaymentAmount
    {
        public string Currency { get; set; }

        public string Value { get; set; }

        public PaymentAmount()
        {
            Currency = "PHP";
        }

        public PaymentAmount(int amount) : this()
        {
            Value = amount.ToString();
        }
    }
}
