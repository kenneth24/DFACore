using DFACore.Helpers;
using DFACore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Helpers.Payment.PaymentDataCache _paymentDataCache;
        private readonly Repository.PaymentRepository _paymentRepository;

        public PaymentController(
            UnionBankClient unionBankClient,
            UserManager<ApplicationUser> userManger,
            Helpers.Payment.PaymentDataCache paymentDataCache,
            Repository.PaymentRepository paymentRepository)
        {
            _unionBankClient = unionBankClient;
            _userManager = userManger;
            _paymentDataCache = paymentDataCache;
            _paymentRepository = paymentRepository;
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
                var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
                if (string.IsNullOrEmpty(main.ApplicationCode))
                {
                    RedirectToAction("SiteSelection", "Home");
                }

                var accessToken = await _unionBankClient.GetCustomerAccountAccessTokenAsync(authorizationCode).ConfigureAwait(false);
                //var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
                //main.TotalFees = 200;
                //HttpContext.Session.SetComplexData("Model", main);

                var merchantPayment = new MerchantPayment
                {
                    SenderRefId = main.ApplicationCode,  //DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    TranRequestDate = DateTime.UtcNow,
                    Amount = new PaymentAmount(Convert.ToInt32(main.TotalFees))
                    //Amount = new PaymentAmount { Value = main.TotalFees.ToString() }
                };

                await _unionBankClient.CreateMerchantPaymentAsync(merchantPayment, accessToken).ConfigureAwait(false);

                main.IsPaymentSuccess = true;

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
                var customerAccountAccessToken = await _unionBankClient.GetCustomerAccountAccessTokenAsync(code).ConfigureAwait(false);
                var requestOtpResult = await _unionBankClient.RequestMerchantPaymentOtpAsync(customerAccountAccessToken).ConfigureAwait(false);
                var paymentData = new Helpers.Payment.PaymentData
                {
                    AccessToken = customerAccountAccessToken,
                    OtpRequestId = requestOtpResult.RequestId
                };

                _paymentDataCache.SetData(_userManager.GetUserId(User), paymentData);

                return View("ConfirmPayment", new ConfirmPaymentViewModel());
            }
            catch (Exception e)
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
                var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");

                if (string.IsNullOrEmpty(main.ApplicationCode))
                {
                    RedirectToAction("SiteSelection", "Home");
                }
                var appcode = main.ApplicationCode.Replace("-", string.Empty);

                var paymentData = _paymentDataCache.GetData(_userManager.GetUserId(User));

                if (paymentData is null)
                {
                    return RedirectToAction("PaymentFailed", "Home");
                }
                else
                {
                    var merchantPayment = new MerchantPayment
                    {
                        SenderRefId = appcode, //DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        TranRequestDate = DateTime.UtcNow,
                        Amount = new PaymentAmount(1), //new PaymentAmount(Convert.ToInt32(main.TotalFees)),
                        RequestId = paymentData.OtpRequestId,
                        Otp = model.Otp
                    };

                    //save data
                    
                    var merchantPaymentResult = await _unionBankClient.CreateV5MerchantPaymentAsync(merchantPayment, paymentData.AccessToken).ConfigureAwait(false);
                    var payment = new Payment
                    {
                        TransactionDate = merchantPayment.TranRequestDate,
                        TransactionReferenceId = merchantPayment.SenderRefId,
                        TransactionId = merchantPaymentResult.Payload.UbpTranId,
                        Amount = decimal.Parse(merchantPayment.Amount.Value),
                        UserId = new Guid(_userManager.GetUserId(User))
                    };

                    _paymentRepository.CreatePayment(payment);

                    main.IsPaymentSuccess = true;
                    HttpContext.Session.SetComplexData("Model", main);

                    return RedirectToAction("PaymentSuccess", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("PaymentFailed", "Home");
            }
        }
    }
}
