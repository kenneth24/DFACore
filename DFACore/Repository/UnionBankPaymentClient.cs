using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UnionBankPayment
{
    public class UnionBankPaymentClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool isDisposed;

        public UnionBankPaymentClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api-uat.unionbankph.com")
            };
        }

        public async Task<string> GetAccessToken(string authorizationCode)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/partners/sb/customers/v1/oauth2/token"))
            {
                var formUrlEncodedContentValue = new Dictionary<string, string>();
                formUrlEncodedContentValue.Add("grant_type", "authorization_code");
                formUrlEncodedContentValue.Add("client_id", "fe667681-2650-4c0b-93fb-c365b2cc0953");
                formUrlEncodedContentValue.Add("code", authorizationCode);
                formUrlEncodedContentValue.Add("redirect_uri", "");

                httpRequestMessage.Content = new FormUrlEncodedContent(formUrlEncodedContentValue);

                //var formData = new Dictionary<string, string>();
                //formData.Add("grant_type", "authorization_code");
                //formData.Add("client_id", "fe667681-2650-4c0b-93fb-c365b2cc0953");
                //formData.Add("code", authorizationCode);
                //formData.Add("redirect_uri", "https://localhost:44332/Home/OrderSummary");

                //httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(formData), null, "application/x-www-form-urlencoded");

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonContent);

                    return jsonObject["access_token"].ToString();
                }
                else
                {
                    var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    return null;
                }
            }
        }

        public async Task<CustomerPaymentResult> CreateCustomerPayment(CreateCustomerPaymentParameter parameter, string accessToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/partners/sb/customers/v3/payments/single"))
            {
                httpRequestMessage.Headers.Add("x-ibm-client-id", "fe667681-2650-4c0b-93fb-c365b2cc0953");
                httpRequestMessage.Headers.Add("x-ibm-client-secret", "J8wF8tY1jM4bN5eN6kO4wM7sP3lB0aG8yN8yT6aQ5aI8sN1aD4");
                httpRequestMessage.Headers.Add("x-partner-id", "5dff2cdf-ef15-48fb-a87b-375ebff415bb");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var json = JsonConvert.SerializeObject(parameter, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                //Content = JsonConvert.SerializeObject(new { content = result, rows = dto }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })

                httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var jsonResult = await httpResponseMessage.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<CustomerPaymentResult>(jsonResult);
                }
                else
                {
                    var jsonResult = await httpResponseMessage.Content.ReadAsStringAsync();
                    return null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class CustomerPaymentResult
    {
        public string Code { get; set; }

        public string SenderRefId { get; set; }

        public string State { get; set; }

        public string Uuid { get; set; }

        public string Discription { get; set; }

        public string Type { get; set; }

        public string Amount { get; set; }

        public string UbpTranId { get; set; }

        public DateTime TranRequestDate { get; set; }

        public DateTime TranFinacleDate { get; set; }
    }

    public class CreateCustomerPaymentParameter
    {
        public CreateCustomerPaymentParameter()
        {
            TranRequestDate = DateTime.UtcNow.ToString("yyyy-dd-MM'T'HH:mm:ss.fff");
            Biller = new PaymentBiller
            {
                Id = "0001",
                Name = "Water Services"
            };

            References = new List<PaymentReference>
            {
                new PaymentReference
                {
                    Index = "1",
                    Name = "Contract Account Number",
                    Value = "9657788143"
                }
            };

            Amount = new PaymentAmount { Currency = "PHP", Value = "1" };
            Remarks = "test";
            Particulars = "test";
        }

        public string SenderRefId { get; set; }

        public string TranRequestDate { get; set; }

        public PaymentAmount Amount { get; set; }

        public string Remarks { get; set; }

        public string Particulars { get; set; }

        public PaymentBiller Biller { get; set; }

        public IEnumerable<PaymentReference> References { get; set; }
    }

    public class PaymentAmount
    {
        public string Currency { get; set; }

        public string Value { get; set; }
    }

    public class PaymentBiller
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class PaymentReference
    {
        public string Index { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
