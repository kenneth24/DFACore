using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using UnionBankApi;

namespace DFACore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly UnionBankClient _unionBankClient;

        public PaymentController(UnionBankClient unionBankClient)
        {
            _unionBankClient = unionBankClient;
        }

        [HttpGet("process-merchant-payment")]
        [Authorize]
        public async Task<IActionResult> ProcessMerchantPayment()
        {
            var hasAuthorizationCode = HttpContext.Request.Query.TryGetValue("code", out StringValues authorizationCode);

            if (!hasAuthorizationCode || string.IsNullOrEmpty(authorizationCode) || string.IsNullOrWhiteSpace(authorizationCode))
            {
                // return error page
                return View();
            }

            try
            {
                var accessToken = await _unionBankClient.GetAccessTokenAsync(authorizationCode).ConfigureAwait(false);
                var merchantPayment = new MerchantPayment
                {
                    SenderRefId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    TranRequestDate = DateTime.UtcNow,
                    Amount = new PaymentAmount(1)
                };

                await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, accessToken).ConfigureAwait(false);

                //return success page
                return View();
            }
            catch (Exception)
            {
                // return error page
                return View();
            }
        }
    }
}
