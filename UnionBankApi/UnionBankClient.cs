using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task<string> GetPartnerAccountAccessTokenAsync(string username, string password)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUri}/partners/v1/oauth2/token"))
            {
                var formUrlEncodedContentValue = new Dictionary<string, string>();
                formUrlEncodedContentValue.Add("grant_type", "password");
                formUrlEncodedContentValue.Add("client_id", _configuration.ClientId);
                formUrlEncodedContentValue.Add("username", username);
                formUrlEncodedContentValue.Add("password", password);
                formUrlEncodedContentValue.Add("scope", "account_inquiry");

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

        public async Task<PartnerAccountTransactionHistory> GetPartnerAccountTransactionHistoryAsync(DateTime fromDate, DateTime toDate, string transactionType, int limit, string accessToken)
        {
            const string queryDateFormat = "yyyy-MM-dd";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUri}/portal/accounts/v1/transactions?fromDate={fromDate.Date.ToString(queryDateFormat)}&toDate={toDate.Date.ToString(queryDateFormat)}&tranType={transactionType}&limit={limit}"))
            {
                httpRequestMessage.Headers.Accept.Add(Utilities.HttpHeaderUtility.CreateJsonContentHeaderValue());
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientId, _configuration.ClientId);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientSecret, _configuration.ClientSecret);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.PartnerId, _configuration.PartnerId);

                httpRequestMessage.Headers.Authorization = Utilities.HttpHeaderUtility.CreateAuthorizationHeaderValue(accessToken);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<PartnerAccountTransactionHistory>(jsonContent);
                }
                else
                {
                    throw new UnionBankApiException(jsonContent);
                }
            }
        }

        public async Task<string> GetCustomerAccountAccessTokenAsync(string authorizationCode)
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

        public async Task<RequestOtpResult> RequestMerchantPaymentOtpAsync(string accessToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUri}/merchants/v5/payments/otp/single"))
            {
                httpRequestMessage.Headers.Accept.Add(Utilities.HttpHeaderUtility.CreateJsonContentHeaderValue());
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientId, _configuration.ClientId);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientSecret, _configuration.ClientSecret);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.PartnerId, _configuration.PartnerId);

                httpRequestMessage.Headers.Authorization = Utilities.HttpHeaderUtility.CreateAuthorizationHeaderValue(accessToken);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<RequestOtpResult>(jsonContent);
                }
                else
                {
                    throw new UnionBankApiException(jsonContent);
                }
            }
        }

        public async Task<MerchantPaymentResult> CreateMerchantPaymentAsync(MerchantPayment payment, string accessToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUri}/merchants/v4/payments/single"))
            {
                httpRequestMessage.Headers.Accept.Add(Utilities.HttpHeaderUtility.CreateJsonContentHeaderValue());
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientId, _configuration.ClientId);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientSecret, _configuration.ClientSecret);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.PartnerId, _configuration.PartnerId);

                httpRequestMessage.Headers.Authorization = Utilities.HttpHeaderUtility.CreateAuthorizationHeaderValue(accessToken);

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
                httpRequestMessage.Content = Utilities.HttpContentUtility.CreateJsonContent(requestJsonContent);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var responseJsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<MerchantPaymentResult>(responseJsonContent, isoDateTimeConverter);
                }
                else
                {
                    throw new UnionBankApiException(responseJsonContent);
                }
            }
        }

        public async Task<MerchantPaymentResult> CreateV5MerchantPaymentAsync(MerchantPayment payment, string accessToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUri}/merchants/v4/payments/single"))
            {
                httpRequestMessage.Headers.Accept.Add(Utilities.HttpHeaderUtility.CreateJsonContentHeaderValue());
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientId, _configuration.ClientId);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.ClientSecret, _configuration.ClientSecret);
                httpRequestMessage.Headers.Add(Utilities.HttpHeaderNames.PartnerId, _configuration.PartnerId);

                httpRequestMessage.Headers.Authorization = Utilities.HttpHeaderUtility.CreateAuthorizationHeaderValue(accessToken);

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
                httpRequestMessage.Content = Utilities.HttpContentUtility.CreateJsonContent(requestJsonContent);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var responseJsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<MerchantPaymentResult>(responseJsonContent, isoDateTimeConverter);
                }
                else
                {
                    throw new UnionBankApiException(responseJsonContent);
                }
            }
        }
    }
}
