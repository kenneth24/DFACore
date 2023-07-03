namespace DFACore.Repository
{
    public class PaymentRepository
    {
        private readonly Data.ApplicationDbContext _applicationDbContext;

        public PaymentRepository(Data.ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void CreatePayment(Models.Payment payment)
        {
            _applicationDbContext.PaymentHistory.Add(payment);
        }
    }
}
