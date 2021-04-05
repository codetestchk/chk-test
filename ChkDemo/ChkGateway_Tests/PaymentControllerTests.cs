using System.Threading.Tasks;
using ChkSDK.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChkSDK.DTOs;
using Moq;
using System.Collections.Generic;
using System;
using ChkSDK.BankProxy;
using ChkSDK;
using ChkGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ChkGateway_Tests
{
    [TestClass]
    public class PaymentControllerTests
    {
        [TestMethod]
        public async Task PostNewPayment_Success_Ok()
        {
            // Setup
            var cardServ = new Mock<ICardValidatorService>();
            // Calls to validate card always go through
            cardServ.Setup(a => a.ValidateCardInfo(It.IsAny<Merch_NewPaymentRequest>()))
                .Returns(new List<CardValidationError>());

            var merchServ = new Mock<IMerchantService>();
            // Gets a random useless merchant bank info
            merchServ.Setup(a => a.GetMerchantPaymentDetails(It.IsAny<Guid>()))
                .ReturnsAsync(new MerchantBankInfo()
                {
                    BankAcc = "anything",
                    BankSort = "anything"
                });

            var transactionServ = new Mock<ITransactionService>();
            // All transactions succeed and return a guid
            transactionServ.Setup(a => a.CreateNewTransaction(It.IsAny<Merch_NewPaymentRequest>()))
                .ReturnsAsync(new Guid());
            transactionServ.Setup(a => a.UpdateTransactionStatus(It.IsAny<Guid>(), It.IsAny<TransactionStateEnum>(), It.IsAny<string>(), It.IsAny<Guid>()));

            var bankProxyServ = new Mock<IBankServiceProxy>();
            bankProxyServ.Setup(a => a.ProcessPayment(It.IsAny<Bank_NewPaymentRequest>()))
                .ReturnsAsync(new Bank_NewPaymentResponse()
                {
                    BankTransactionID = new Guid(),
                    Message = "Success",
                    TransactionState = TransactionStateEnum.Succeeded
                });

            var paymentController = new PaymentController(null, bankProxyServ.Object, merchServ.Object, transactionServ.Object, cardServ.Object);

            // Action
            var result = await paymentController.PostNewPayment(new Merch_NewPaymentRequest()
            {
                Amount = 12.53M,
                CardCVV = 123,
                CardExpMonth = 12,
                CardExpYear = 2023,
                CardNameOnCard = "blah",
                CardNumber = "anyhting",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = new Guid()
            });

            // Assert
            // Testing that when a new payment is added successfully, it returns OK status code
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_NewPaymentResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task PostNewPayment_Failed_CardValidation_BadRequest()
        {
            // Setup
            var cardServ = new Mock<ICardValidatorService>();
            // Calls to validate card always go through
            cardServ.Setup(a => a.ValidateCardInfo(It.IsAny<Merch_NewPaymentRequest>()))
                .Returns(new List<CardValidationError>() {
                    new CardValidationError() {
                        Advice = "Put in card num accurately",
                        FieldName = "Card Number",
                        ValidationFailureMessage = "Card number is not 16 digits" }
                });

            var merchServ = new Mock<IMerchantService>();
            // Gets a random useless merchant bank info
            merchServ.Setup(a => a.GetMerchantPaymentDetails(It.IsAny<Guid>()))
                .ReturnsAsync(new MerchantBankInfo()
                {
                    BankAcc = "anything",
                    BankSort = "anything"
                });

            var transactionServ = new Mock<ITransactionService>();
            // All transactions succeed and return a guid
            transactionServ.Setup(a => a.CreateNewTransaction(It.IsAny<Merch_NewPaymentRequest>()))
                .ReturnsAsync(new Guid());
            transactionServ.Setup(a => a.UpdateTransactionStatus(It.IsAny<Guid>(), It.IsAny<TransactionStateEnum>(), It.IsAny<string>(), It.IsAny<Guid>()));

            var bankProxyServ = new Mock<IBankServiceProxy>();
            bankProxyServ.Setup(a => a.ProcessPayment(It.IsAny<Bank_NewPaymentRequest>()))
                .ReturnsAsync(new Bank_NewPaymentResponse()
                {
                    BankTransactionID = new Guid(),
                    Message = "Success",
                    TransactionState = TransactionStateEnum.Succeeded
                });

            var paymentController = new PaymentController(null, bankProxyServ.Object, merchServ.Object, transactionServ.Object, cardServ.Object);

            // Action
            var result = await paymentController.PostNewPayment(new Merch_NewPaymentRequest()
            {
                Amount = 12.53M,
                CardCVV = 123,
                CardExpMonth = 12,
                CardExpYear = 2023,
                CardNameOnCard = "blah",
                CardNumber = "anyhting",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = new Guid()
            });

            // Assert
            // Testing that when an incorrect card number is put into the API, it returns bad request
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_NewPaymentResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetExistingPayment_Success_PaymentFound_OK()
        {
            // Setup
            var transactionServ = new Mock<ITransactionService>();
            // All transactions succeed and return a guid
            transactionServ.Setup(a => a.GetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Merch_GetPaymentResponse()
                {
                    Amount = 12.22M,
                    CardExpMonth = 12,
                    CardExpYear = 2022,
                    CardNameOnCard = "blah",
                    CardNumber = "123",
                    CurrencyID = (int)CurrencyEnum.EUR,
                    DateTimeStateLastUpdated = DateTime.UtcNow,
                    StateID = (int)TransactionStateEnum.Succeeded,
                    StateMessage = "Success"
                });

            var paymentController = new PaymentController(null, null, null, transactionServ.Object, null);
            // TODO, not the greatest to setup a HttpContext on controller during unit testing, should be improved with refactoring
            // controller and how it accesses context in a less depdent way
            paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            paymentController.ControllerContext.HttpContext.Items["MerchantID"] = Guid.NewGuid();

            // Action
            var result = await paymentController.GetExistingPayment(new Merch_GetPaymentRequest()
            {
                PaymentID = new Guid()
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_GetPaymentResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetExistingPayment_Failed_NoMerchantHttpContextSet_Unauthorized()
        {
            // Setup
            var transactionServ = new Mock<ITransactionService>();
            // All transactions succeed and return a guid
            transactionServ.Setup(a => a.GetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Merch_GetPaymentResponse()
                {
                    Amount = 12.22M,
                    CardExpMonth = 12,
                    CardExpYear = 2022,
                    CardNameOnCard = "blah",
                    CardNumber = "123",
                    CurrencyID = (int)CurrencyEnum.EUR,
                    DateTimeStateLastUpdated = DateTime.UtcNow,
                    StateID = (int)TransactionStateEnum.Succeeded,
                    StateMessage = "Success"
                });

            var paymentController = new PaymentController(null, null, null, transactionServ.Object, null);
            // Here we do NOT setup the HTTP context with the Merchant ID, so it's essentially unauthorized

            // Action
            var result = await paymentController.GetExistingPayment(new Merch_GetPaymentRequest()
            {
                PaymentID = new Guid()
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_GetPaymentResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetExistingPayment_Failed_WrongMerchantForPayment_Unauthorized()
        {
            // Setup
            var transactionServ = new Mock<ITransactionService>();
            // All transactions will return null
            transactionServ.Setup(a => a.GetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Merch_GetPaymentResponse)null);
            // Apart from one specific merch id payment id pair
            transactionServ.Setup(a => a.GetTransaction(It.Is<Guid>(b => b == new Guid("fcccb868-9094-49f7-afe8-fe48e8b84fee")), It.Is<Guid>(c => c == new Guid("2b03a339-4d10-4a9b-b788-bf0b679555f0"))))
                .ReturnsAsync(new Merch_GetPaymentResponse()
                {
                    Amount = 12.22M,
                    CardExpMonth = 12,
                    CardExpYear = 2022,
                    CardNameOnCard = "blah",
                    CardNumber = "123",
                    CurrencyID = (int)CurrencyEnum.EUR,
                    DateTimeStateLastUpdated = DateTime.UtcNow,
                    StateID = (int)TransactionStateEnum.Succeeded,
                    StateMessage = "Success"
                });

            var paymentController = new PaymentController(null, null, null, transactionServ.Object, null);
            // TODO, not the greatest to setup a HttpContext on controller during unit testing, should be improved with refactoring
            // controller and how it accesses context in a less depdent way
            paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            // note, MerchantID is WRONG, does not match that of the payment
            paymentController.ControllerContext.HttpContext.Items["MerchantID"] = Guid.NewGuid();

            // Action
            // Note payment ID is right, but it's for a different Merchant
            var result = await paymentController.GetExistingPayment(new Merch_GetPaymentRequest()
            {
                PaymentID = new Guid("fcccb868-9094-49f7-afe8-fe48e8b84fee")
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_GetPaymentResponse>));
            // When the merchant id and payment id does not match, should return not found, rather than 'unauthorized/forbidden', to help
            // hide even more information, don't confirm payment ID exists
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetExistingPayment_Failed_NonExistantPayment_NotFound()
        {
            // Setup
            var transactionServ = new Mock<ITransactionService>();
            // All transactions will return null
            transactionServ.Setup(a => a.GetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Merch_GetPaymentResponse)null);
            // Apart from one specific merch id payment id pair
            transactionServ.Setup(a => a.GetTransaction(It.Is<Guid>(b => b == new Guid("fcccb868-9094-49f7-afe8-fe48e8b84fee")), It.Is<Guid>(c => c == new Guid("2b03a339-4d10-4a9b-b788-bf0b679555f0"))))
                .ReturnsAsync(new Merch_GetPaymentResponse()
                {
                    Amount = 12.22M,
                    CardExpMonth = 12,
                    CardExpYear = 2022,
                    CardNameOnCard = "blah",
                    CardNumber = "123",
                    CurrencyID = (int)CurrencyEnum.EUR,
                    DateTimeStateLastUpdated = DateTime.UtcNow,
                    StateID = (int)TransactionStateEnum.Succeeded,
                    StateMessage = "Success"
                });

            var paymentController = new PaymentController(null, null, null, transactionServ.Object, null);
            // TODO, not the greatest to setup a HttpContext on controller during unit testing, should be improved with refactoring
            // controller and how it accesses context in a less depdent way
            paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            // note, MerchantID is right
            paymentController.ControllerContext.HttpContext.Items["MerchantID"] = new Guid("2b03a339-4d10-4a9b-b788-bf0b679555f0");

            // Action
            // Note payment ID is wrong, doesn't match any existing payment
            var result = await paymentController.GetExistingPayment(new Merch_GetPaymentRequest()
            {
                PaymentID = Guid.NewGuid()
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_GetPaymentResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
    }
}
