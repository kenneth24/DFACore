using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace UnionBankApi.UnitTests
{
    [TestFixture]
    public class ClientTests
    {
        private const string PartnerAccountUsername = "partner_sb";
        private const string PartnerAccountPassword = "p@ssw0rd";

        private UnionBankClient _unionBankClient;

        [SetUp]
        public void SetUp()
        {
            var unionBankClientConfiguration = new UnionBankClientConfiguration
            {
                BaseUri = "https://api-uat.unionbankph.com/partners/sb",
                ClientId = "fe667681-2650-4c0b-93fb-c365b2cc0953",
                ClientSecret = "J8wF8tY1jM4bN5eN6kO4wM7sP3lB0aG8yN8yT6aQ5aI8sN1aD4",
                PartnerId = "5dff2cdf-ef15-48fb-a87b-375ebff415bb"
            };

            _unionBankClient = new UnionBankClient(unionBankClientConfiguration);
        }

        [Test]
        public async Task GetCustomerAccountAccessTokenTest()
        {
            //Request authorization code
            //https://api-uat.unionbankph.com/partners/sb/customers/v1/oauth2/authorize?response_type=code&client_id=fe667681-2650-4c0b-93fb-c365b2cc0953&scope=payments&type=single&partnerId=5dff2cdf-ef15-48fb-a87b-375ebff415bb

            var accessToken = await _unionBankClient.GetCustomerAccountAccessTokenAsync("AAK80_Q0Irt-LrepHa8ha7hcdS1SdFrJumrZk_RWBMgf8YKz6ohIQz1hgwnt5tFVi8ogsifNAQuV2odIDBNG9oSHndVAYvP-3ugJfiw45HLwOTHfr5pyj0oz3J4hLCLdv_DMS30zfWqRnZgz9HYdxs-67qcL4NHPH_VzSegwIo8UUAPSK9HyQkAllSXfg_UNSbf6y2mXNbb3wzcmyGK1hmfkesMUS6flMehIHSfS5UsesZ5ROZnowUO_dSizA45tqMot7JRMX8t5j2X4S9Oa2v6HVAwk2QRPTYgHp5X-mA3-c_v_L4iE8bpGXl8Mr2l79lAl4TjMFIZPy2pvA_NqsoLhepUrVzH8DQFrX-RgFPtubw");

            Assert.IsNotNull(accessToken);
        }

        [Test]
        public async Task RequestOtpAsyncTest()
        {
            var requestOtpResult = await _unionBankClient.RequestMerchantPaymentOtpAsync("AAIkZmU2Njc2ODEtMjY1MC00YzBiLTkzZmItYzM2NWIyY2MwOTUzJVh1x5xoruP8hRosJBA5g-LZnB-bb_hK8VP_vY0-NpGjiBXkowsFvvBHk8bvQ2mmf5bwalk5mfJdMcMDDpxfrSBDipyjZrpveL9G2FBpmox-GHwy1i1ygzRPmHYJDN0e-F0dBgyxuN80qfoSqIwkA5Z8OpUbGyuy89UaPFT5xrwRx_2F3y_1BKe2atQ63mzQVe5YTrFeNa2M3omlj6e5vOM8JcGEIf8PdbvWHka2jClt5IH3eyDVFfosaAc9V9NGriprKbMAiJO-cLn6vcS5SX9BHgnLB3pAYYMoTcBbqN9o7uSb6_ZeiTwyYR1KjE7D6Olag0KXP5SAqpGy3j7S8H1D_4PZ14fk-vzxmBEcTE8");

            Assert.IsNotNull(requestOtpResult);
        }

        [Test]
        public async Task CreateMerchantPaymentTest()
        {
            var merchantPayment = new MerchantPayment
            {
                SenderRefId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                TranRequestDate = DateTime.UtcNow,
                Amount = new PaymentAmount
                {
                    Currency = "PHP",
                    Value = "1"
                },
                RequestId = "",
                Otp = "222222"
            };

            var merchantPaymentResult = await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, "AAIkZmU2Njc2ODEtMjY1MC00YzBiLTkzZmItYzM2NWIyY2MwOTUzWs8Ejv1dL8g6goHjppL1iXHsGQq4IKp15BUyuNkvqtcxWzBg39WxIuGrSHIRhzkmLWng1ntJp-0q5eJ8L0ghoUP13e7VLeeUw_AhnOA8kuYaJ5HMyrY71rtQG2dAyYlP5id5WlDva53757QvcklYJwt7NTsd_uUriBLH6FV9GAyiwFeT1kU-tnZlzIB4LMFxx7FKcOZDwjDhXz5Omx3ke7ZhHSTd8ilHFQRCBjEPbqghn-SDi48zMqmICbNgA8VGufwjPMNnwoAb69NJaqD4iJKJtwO-3EelTX4bHubKH1fci02APo5lAnltJ7WMKUxEBMEMScxeHqCig8TA0iPQFh00UFGNYxR6MCBgISf1bqQ");

            Assert.IsNotNull(merchantPaymentResult);
            Assert.AreEqual(merchantPayment.SenderRefId, merchantPaymentResult.Payload.SenderRefId);
            Assert.AreEqual(merchantPayment.TranRequestDate, merchantPaymentResult.Payload.TranRequestDate);
        }

        [Test]
        public async Task GetPartnerAccountAccessTokenTest()
        {
            var accessToken = await _unionBankClient.GetPartnerAccountAccessTokenAsync(PartnerAccountUsername, PartnerAccountPassword);

            Assert.IsNotNull(accessToken);
        }

        [Test]
        public async Task GetPartnerAccountTransactionHistoryTest()
        {
            var accessToken = await _unionBankClient.GetPartnerAccountAccessTokenAsync(PartnerAccountUsername, PartnerAccountPassword);
            var partnerAccountTransactionHistory = await _unionBankClient.GetPartnerAccountTransactionHistoryAsync(new DateTime(2017, 1, 1), new DateTime(2017, 12, 31), PartnerAccountTransactionType.Debit, 4, accessToken);

            Assert.IsNotNull(partnerAccountTransactionHistory);
        }
    }
}
