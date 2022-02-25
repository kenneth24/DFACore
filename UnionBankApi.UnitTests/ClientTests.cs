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

            var accessToken = await _unionBankClient.GetCustomerAccountAccessTokenAsync("AAKUyw409rVRhpvjpTVBm5amKW6J4mKA28N_kZGBTSLXa5grsMHcUKsaba2LEUVQqY-vgZj59UF7WruvPqMioLfuUFRzg_oZvtBDNN6qkgdug1OA7QcGaM65qeUbFeukeZCPHbELMYvbxY2Pn1-6TKCH-YA5pjy-anrlSkAliGXD9_odToWnqeLqkYc5WM8yNETkaFnoch2JrrDr2IgcL1LxaK1YpCu9rCiKSU41uFbSOJ4zeGBFACdcC9gipVi17uMxlH1mI4NLcl_nKtTPN2ZbaJUfQFpwBvDTRNFxv2-duno18MjmY8ldEuT-2ozP4ggqHikepye6Ii6Em6aRT6wjkddD4SklicUZsMktTKEqug");

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
                Otp = "222221"
            };

            var merchantPaymentResult = await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, "AAIkZmU2Njc2ODEtMjY1MC00YzBiLTkzZmItYzM2NWIyY2MwOTUz4rQcHhpEbKhHy6irQVBb-jfdfUXPHKB7jlXce8PXG3xLPOBPMuMuH8-QDkkzMyBY1NNkByYZVa4k9IPjFyqaAiV7Q8sHRsZNFoMnz4h7g9bqTZescj-PGSbpD8d7FMgLQsga6QePOaCWiB0LT0FJkZS0775kF1zjO0JAw4SS13BdV81Ervn5Rj_xBsxljDPrcmRXGqDz9U5VK5oFxLiEiPjYA_JSqiz5baxAl-0nW0qDDNf8lOcrpQh_M71GxYmJ6APn-OCqGOKvOOBL3bZbMGc7QCyqf7pVUGEwKy3zsK1ojqgdnVRcFG_qlgOwh0tfV8e9K9J4ZgvcZe3sq98vLTEpX0igB1XL4jTBthFaaWs");

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
