using DFACore.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class GoogleCaptchaService
    {
        private ReCAPTCHASetting _settings;
        public GoogleCaptchaService(IOptions<ReCAPTCHASetting> settings)
        {
            _settings = settings.Value;
        }

        public virtual async Task<GoogleResponse> VerifyReCaptcha(string token)
        {
            var data = new GoogleCaptchaData()
            {
                response = token,
                secret = _settings.ReCAPTCHA_Secret_Key
            };
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={data.secret}&response={data.response}");
            var result = JsonConvert.DeserializeObject<GoogleResponse>(response);
            return result;
        }
    }

    public class GoogleCaptchaData
    {
        public string response { get; set; }
        public string secret { get; set; }
    }

    public class GoogleResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
