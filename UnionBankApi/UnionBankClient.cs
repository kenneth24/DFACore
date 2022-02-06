using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UnionBankApi
{
    public class UnionBankClient
    {
        private readonly UnionBankClientConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private bool isDisposed;

        public UnionBankClient(UnionBankClientConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
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

        public async Task<string> GetAccessTokenAsync(string authorizationCode)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUri}/customers/v1/oauth2/token"))
            {
                var formUrlEncodedContentValue = new Dictionary<string, string>();
                formUrlEncodedContentValue.Add("grant_type", "authorization_code");
                formUrlEncodedContentValue.Add("client_id", _configuration.ClientId);
                formUrlEncodedContentValue.Add("code", authorizationCode);

                httpRequestMessage.Content = new FormUrlEncodedContent(formUrlEncodedContentValue);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var jsonObject = JObject.Parse(jsonContent);

                    return jsonObject["access_token"].ToString();
                }
                else
                {
                    throw new UnionBankApiException(jsonContent);
                }
            }
        }

        public async Task<MerchantPaymentResult> CreateMerchantPaymentAsync(MerchantPayment payment, string accessToken)
        {
            const string jsonContentType = "application/json";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUri}/merchants/v4/payments/single"))
            {
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(jsonContentType));
                httpRequestMessage.Headers.Add("x-ibm-client-id", _configuration.ClientId);
                httpRequestMessage.Headers.Add("x-ibm-client-secret", _configuration.ClientSecret);
                httpRequestMessage.Headers.Add("x-partner-id", _configuration.PartnerId);

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var isoDateTimeConverter = new IsoDateTimeConverter
                {
                    DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff"
                };

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

                jsonSerializerSettings.Converters.Add(isoDateTimeConverter);

                var requestJsonContent = JsonConvert.SerializeObject(payment, jsonSerializerSettings);
                httpRequestMessage.Content = new StringContent(requestJsonContent, Encoding.UTF8, jsonContentType);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var responsejsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<MerchantPaymentResult>(responsejsonContent, isoDateTimeConverter);
                }
                else
                {
                    throw new UnionBankApiException(responsejsonContent);
                }
            }
        }
    }
}
