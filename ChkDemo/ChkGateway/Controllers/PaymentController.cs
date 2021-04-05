using System;
using System.Threading.Tasks;
using ChkGateway.AttributeFilters;
using ChkSDK.BankProxy;
using ChkSDK.DTOs;
using ChkSDK.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChkGateway.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IBankServiceProxy _bankServiceProxy;
        private readonly IMerchantService _merchantService;
        private readonly ITransactionService _transactionService;
        private readonly ICardValidatorService _vardValidatorService;

        public PaymentController(ILogger<PaymentController> logger, IBankServiceProxy bankServiceProxy, IMerchantService merchantService, ITransactionService transactionService, ICardValidatorService cardValidatorService)
        {
            // TODO use logger
            _logger = logger;
            _bankServiceProxy = bankServiceProxy;
            _merchantService = merchantService;
            _transactionService = transactionService;
            _vardValidatorService = cardValidatorService;
        }

        [HttpPost]
        [AuthorizeMerchant]
        public async Task<ActionResult<Merch_NewPaymentResponse>> PostNewPayment(Merch_NewPaymentRequest paymentRequest)
        {
            var validationErrors = _vardValidatorService.ValidateCardInfo(paymentRequest);
            if(validationErrors.Count >= 1)
            {
                return BadRequest(validationErrors);
            }

            var guidOfTransaction = await _transactionService.CreateNewTransaction(paymentRequest);

            MerchantBankInfo destMerchInfo = await _merchantService.GetMerchantPaymentDetails(paymentRequest.MerchantID);

            Bank_NewPaymentRequest bankRequest = new Bank_NewPaymentRequest()
            {
                DestinationMerchantInfo = destMerchInfo,
                SourcePaymentInfo = new SourcePaymentInfo()
                {
                    Amount = paymentRequest.Amount,
                    CardCVV = paymentRequest.CardCVV,
                    CardExpMonth = paymentRequest.CardExpMonth,
                    CardExpYear = paymentRequest.CardExpYear,
                    CardNameOnCard = paymentRequest.CardNameOnCard,
                    CardNumber = paymentRequest.CardNumber,
                    CurrencyID = paymentRequest.CurrencyID
                }
            };

            var bankProcessResult = await _bankServiceProxy.ProcessPayment(bankRequest);

            // When bank has finished processing, update transaction state with bank result, also pair transaction with bank Transaction GUID
            await _transactionService.UpdateTransactionStatus(guidOfTransaction, bankProcessResult.TransactionState, bankProcessResult.Message, bankProcessResult.BankTransactionID);

            return Ok(new Merch_NewPaymentResponse()
            {
                Message = bankProcessResult.Message,
                TransactionState = bankProcessResult.TransactionState,
                TransactionID = guidOfTransaction
            });
        }


        // Note: I don't particularly want the Payment GUID to be in the URL, so I chose HttpPost's for all communications in this solution,
        // rather than going 'restful' approach using Http Verbs etc.
        [AuthorizeMerchant]
        [HttpPost]
        public async Task<ActionResult<Merch_GetPaymentResponse>> GetExistingPayment(Merch_GetPaymentRequest getPaymentRequest)
        {
            // Merchant ID should be set in the HttpContext as key 'MerchantID' this is set/controller by the JwtMiddleware that intercepts every request
            var merchID = GetMerchantIDFromContext(HttpContext);

            // Should that Merchant ID not be found, it is classed as unauthorized, logically impossible to get here, but belt and braces 
            if(merchID == null || merchID == Guid.Empty)
            {
                return Unauthorized();
            }

            // The reason why we need to extract this merchantID from the context is because we need to validate the merchant has access to this particular requested payment ID
            // This is so that ANY authenticated merchant cannot just get access to ANY payment ID
            var payment = await _transactionService.GetTransaction(getPaymentRequest.PaymentID, merchID);
            if(payment == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(payment);
            }
        }

        private Guid GetMerchantIDFromContext(HttpContext httpContext)
        {
            try
            {
                if (httpContext.Items["MerchantID"] != null)
                {
                    return (Guid)httpContext.Items["MerchantID"];
                }
            }
            catch
            {
                return Guid.Empty;
            }
            return Guid.Empty;
        }
    }
}
