using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace UnionBankApi.UnitTests
{
    [TestFixture]
    public class ClientTests
    {
        //sandbox
        //private const string PartnerAccountUsername = "partner_sb";
        //private const string PartnerAccountPassword = "p@ssw0rd";

        private const string PartnerAccountUsername = "omni_uat";
        private const string PartnerAccountPassword = "0mniu@T0Gvv#$u6xxS!H44I";

        private UnionBankClient _unionBankClient;

        [SetUp]
        public void SetUp()
        {
            //sandbox
            //var unionBankClientConfiguration = new UnionBankClientConfiguration
            //{
            //    BaseUri = "https://api-uat.unionbankph.com/partners/sb",
            //    ClientId = "fe667681-2650-4c0b-93fb-c365b2cc0953",
            //    ClientSecret = "J8wF8tY1jM4bN5eN6kO4wM7sP3lB0aG8yN8yT6aQ5aI8sN1aD4",
            //    PartnerId = "5dff2cdf-ef15-48fb-a87b-375ebff415bb"
            //};

            //uat
            var unionBankClientConfiguration = new UnionBankClientConfiguration
            {
                BaseUri = "https://api-uat.unionbankph.com/ubp/external",
                ClientId = "608e9f9c-b412-49ae-9d02-7ae6e23d1764",
                ClientSecret = "X5oF8rK7mJ0uK0uE5sE4rL0jH1nQ5oV7vO0gA4tI7rY3aU3wU4",
                PartnerId = "4af3d4f5-1bba-4cf5-8715-8bab7af59229"
            };

            _unionBankClient = new UnionBankClient(unionBankClientConfiguration);
        }

        [Test]
        public async Task GetCustomerAccountAccessTokenTest()
        {
            //sandbox request authorization code
            //https://api-uat.unionbankph.com/partners/sb/customers/v1/oauth2/authorize?response_type=code&client_id=fe667681-2650-4c0b-93fb-c365b2cc0953&scope=payments&type=single&partnerId=5dff2cdf-ef15-48fb-a87b-375ebff415bb

            //uat request authorization code
            //https://api-uat.unionbankph.com/ubp/external/customers/v1/oauth2/authorize?response_type=code&client_id=608e9f9c-b412-49ae-9d02-7ae6e23d1764&scope=payments&type=single&partnerId=4af3d4f5-1bba-4cf5-8715-8bab7af59229

            var accessToken = await _unionBankClient.GetCustomerAccountAccessTokenAsync("AAKkxJK_1-s3_WmdrEpqtuzbVFueO96vAMjeRAk8gK07Te5-STtAV_sezCyWW0qz5iugyPCOf4h4h99nmZpWt4jQhM14055IRfrS_Xy5sBSEe_pGuRbpmm511mlPXZr2pdti8r4gx7HftuxbowENktv4B4hfb3tPjdnnBqoPD2gOyxT7Ytr-ba8G6ZINlgptrjbXjuh45126sXjmc5lskTv1Tkl8LpB0ZDInikOz77yXEgzTMf8Xn3zxV_qRSAL2dQfOpIDRiMCV0cvoa9jH0MvtVFas5ESDiLwPMaRhDE_P4u9aeMchArvEs4CwSTs6uuo7htGbSM-EPO_pAXrlOTIhcblgj2xg_X-WAQ4hweGZHw");

            Assert.IsNotNull(accessToken);
        }

        [Test]
        public async Task RequestOtpAsyncTest()
        {
            var requestOtpResult = await _unionBankClient.RequestMerchantPaymentOtpAsync("AAIkNjA4ZTlmOWMtYjQxMi00OWFlLTlkMDItN2FlNmUyM2QxNzY0EnzCsiDUrGhjuchWQ81iNvXcxlmwtTuuGJUQ2wRvvZbsrA8uhyooFmmbo6aWZSVMx5dtLtnPC-UKpLe9dxw66Ej-b_U4SRMalxhfUIiGaFXLOqErGjj7wPRU00JLq55HmdeSJSkFVdnjL13kyyN-zRTJwDCjuQ1gcTwW09E69x1G_sDXFV97G356vO1SInXPlrj7krnrqM5Sz8-dDrSu9d67dzoBF1pTtAisxlfBZfwt8ruUNfKDgXcs5TqAjJvadlhZKWn1mAgQTM_PLdb8sMJyT8oRIbJ6vHIPS2xf_kw8epLInOa-1jqFsu9Ej5Hkmknk-QcKnG_6nCaXos_4UNUvaN1oIdo3jjS76Fp5DME");

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
                RequestId = "1648274312924639266078027",
                Otp = "111111"
            };

            var merchantPaymentResult = await _unionBankClient.CreateV5MerchantPaymentAsync(merchantPayment, "AAIkNjA4ZTlmOWMtYjQxMi00OWFlLTlkMDItN2FlNmUyM2QxNzY0EnzCsiDUrGhjuchWQ81iNvXcxlmwtTuuGJUQ2wRvvZbsrA8uhyooFmmbo6aWZSVMx5dtLtnPC-UKpLe9dxw66Ej-b_U4SRMalxhfUIiGaFXLOqErGjj7wPRU00JLq55HmdeSJSkFVdnjL13kyyN-zRTJwDCjuQ1gcTwW09E69x1G_sDXFV97G356vO1SInXPlrj7krnrqM5Sz8-dDrSu9d67dzoBF1pTtAisxlfBZfwt8ruUNfKDgXcs5TqAjJvadlhZKWn1mAgQTM_PLdb8sMJyT8oRIbJ6vHIPS2xf_kw8epLInOa-1jqFsu9Ej5Hkmknk-QcKnG_6nCaXos_4UNUvaN1oIdo3jjS76Fp5DME");

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
