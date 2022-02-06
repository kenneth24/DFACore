using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace UnionBankApi.UnitTests
{
    [TestFixture]
    public class ApiTests
    {
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
        public async Task GetAccessTokenTest()
        {
            //Request authorization code
            //https://api-uat.unionbankph.com/partners/sb/customers/v1/oauth2/authorize?response_type=code&client_id=fe667681-2650-4c0b-93fb-c365b2cc0953&scope=payments&type=single&partnerId=5dff2cdf-ef15-48fb-a87b-375ebff415bb

            var accessToken = await _unionBankClient.GetAccessTokenAsync("AAKO38u5iPtcbCuMzdFrzWaYRdgawfEDFV7IjrMyKypv_gm8T_ZljugWqwFx0AxTGb_eXXU6SiYGgatoHaUHa3XT7o3Epf_QP54OVT-wkpNWiXWgIOXTsgCjfE4A2sx6DfmdRE-rMYOS2-HHMZxXLU3OOfzG-C_6y1B7VbI80W87IBQpb0ez35W7K-JjsKtLXlQIlsgP8e_IbVr8dIQ1SVcrsQFt97DzsV4aPBLU_ybNyS-nM6QlJnQssXroMu8xF0qnXHvdylPNW4hUJ2IuF3_n6-XOxdMyMj3O5JEHD_zDjk9GsOcXQpirkHa5QNuQ32PztXzGcUx7pq30EsOjAs1YutPjkZ2uF05AP_kPQkIhZA");

            Assert.IsNotNull(accessToken);
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
                }
            };

            var merchantPaymentResult = await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, "AAIkZmU2Njc2ODEtMjY1MC00YzBiLTkzZmItYzM2NWIyY2MwOTUzWs8Ejv1dL8g6goHjppL1iXHsGQq4IKp15BUyuNkvqtcxWzBg39WxIuGrSHIRhzkmLWng1ntJp-0q5eJ8L0ghoUP13e7VLeeUw_AhnOA8kuYaJ5HMyrY71rtQG2dAyYlP5id5WlDva53757QvcklYJwt7NTsd_uUriBLH6FV9GAyiwFeT1kU-tnZlzIB4LMFxx7FKcOZDwjDhXz5Omx3ke7ZhHSTd8ilHFQRCBjEPbqghn-SDi48zMqmICbNgA8VGufwjPMNnwoAb69NJaqD4iJKJtwO-3EelTX4bHubKH1fci02APo5lAnltJ7WMKUxEBMEMScxeHqCig8TA0iPQFh00UFGNYxR6MCBgISf1bqQ");

            Assert.IsNotNull(merchantPaymentResult);
            Assert.AreEqual(merchantPayment.SenderRefId, merchantPaymentResult.Payload.SenderRefId);
            Assert.AreEqual(merchantPayment.TranRequestDate, merchantPaymentResult.Payload.TranRequestDate);
        }
    }
}
