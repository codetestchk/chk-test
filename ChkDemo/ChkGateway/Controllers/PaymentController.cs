﻿using System.Threading.Tasks;
using ChkSDK.BankProxy;
using ChkSDK.DTOs;
using ChkSDK.Services;
using ChkSDK.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChkGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IBankServiceProxy _bankServiceProxy;
        private readonly IMerchantService _merchantService;
        private readonly ITransactionService _transactionService;

        public PaymentController(ILogger<PaymentController> logger, IBankServiceProxy bankServiceProxy, IMerchantService merchantService, ITransactionService transactionService)
        {
            // TODO use logger
            _logger = logger;
            _bankServiceProxy = bankServiceProxy;
            _merchantService = merchantService;
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<ActionResult<Merch_NewPaymentResponse>> PostNewPayment(Merch_NewPaymentRequest paymentRequest)
        {
            var validationErrors = CardValidator.ValidateCardInfo(paymentRequest);
            if(validationErrors.Count >= 1)
            {
                return BadRequest(validationErrors);
            }

            if(await _merchantService.ValidateIDApiKey(paymentRequest.MerchantID, paymentRequest.APIKey) == false)
            {
                return Unauthorized();
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
            await _transactionService.UpdateTransactionStatus(guidOfTransaction, bankProcessResult.TransactionState, bankProcessResult.Message, bankProcessResult.BankTransactionID);

            return Ok(new Merch_NewPaymentResponse()
            {
                Message = bankProcessResult.Message,
                TransactionState = bankProcessResult.TransactionState,
                TransactionID = guidOfTransaction
            });
;        }
    }
}
