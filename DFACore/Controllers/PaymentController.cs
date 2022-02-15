using DFACore.Helpers;
using DFACore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using UnionBankApi;

namespace DFACore.Controllers
{
    [Authorize]
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
                return RedirectToAction("PaymentFailed", "Home");
            }

            try
            {
                var accessToken = await _unionBankClient.GetAccessTokenAsync(authorizationCode).ConfigureAwait(false);
                //var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
                //main.TotalFees = 200;
                //HttpContext.Session.SetComplexData("Model", main);

                var merchantPayment = new MerchantPayment
                {
                    SenderRefId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    TranRequestDate = DateTime.UtcNow,
                    Amount = new PaymentAmount(1)
                    //Amount = new PaymentAmount { Value = main.TotalFees.ToString() }
                };

                await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, accessToken).ConfigureAwait(false);

                //return success page
                return RedirectToAction("PaymentSuccess", "Home");
                //return View();
            }
            catch (Exception)
            {
                // return error page
                return RedirectToAction("PaymentFailed", "Home");
            }
        }

        //[AllowAnonymous]
        [HttpGet("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction("PaymentFailed", "Home");
            }

            try
            {
                var accessToken = await _unionBankClient.GetAccessTokenAsync(code).ConfigureAwait(false);
                var requestOtpResult = await _unionBankClient.RequestOtpAsync(accessToken).ConfigureAwait(false);
                var confirmPaymentViewModel = new ConfirmPaymentViewModel
                {
                    PaymentToken = accessToken,
                    PaymentRequestId = requestOtpResult.RequestId
                };

                return View("ConfirmPayment", confirmPaymentViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("PaymentFailed", "Home");
            }
        }

        //[AllowAnonymous]
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(ConfirmPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfirmPayment", model);
            }

            try
            {
                var merchantPayment = new MerchantPayment
                {
                    SenderRefId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    TranRequestDate = DateTime.UtcNow,
                    Amount = new PaymentAmount(1),
                    RequestId = model.PaymentRequestId,
                    Otp = model.Otp
                };

                await _unionBankClient.CreateV5MerchantPaymentAsync(merchantPayment, model.PaymentToken).ConfigureAwait(false);

                return RedirectToAction("PaymentSuccess", "Home");
            }
            catch (Exception)
            {
                return RedirectToAction("PaymentFailed", "Home");
            }
        }
    }
}
