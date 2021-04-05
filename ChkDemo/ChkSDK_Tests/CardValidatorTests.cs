using System;
using ChkSDK;
using ChkSDK.DTOs;
using ChkSDK.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChkSDK_Tests
{
    [TestClass]
    public class CardValidatorTests
    {
        ICardValidatorService cardValidator = new CardValidatorService();

        [TestMethod]
        public void CardValidation_Pass()
        {
            var dataIn = new Merch_NewPaymentRequest()
            {
                Amount = 15.23M,
                CardCVV = 123,
                CardExpMonth = 11,
                CardExpYear = 2026,
                CardNameOnCard = "ABC",
                CardNumber = "1234123412341234",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = Guid.NewGuid()
            };

            Assert.AreEqual(0, cardValidator.ValidateCardInfo(dataIn).Count);
        }

        [TestMethod]
        public void CardValidation_Fail_CardNumber_TooLong()
        {
            var dataIn = new Merch_NewPaymentRequest()
            {
                Amount = 15.23M,
                CardCVV = 123,
                CardExpMonth = 11,
                CardExpYear = 2026,
                CardNameOnCard = "ABC",
                CardNumber = "12341234123412345",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = Guid.NewGuid()
            };

            var result = cardValidator.ValidateCardInfo(dataIn);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Card Number", result[0].FieldName);
        }

        [TestMethod]
        public void CardValidation_Fail_CardNumber_TooShort()
        {
            var dataIn = new Merch_NewPaymentRequest()
            {
                Amount = 15.23M,
                CardCVV = 123,
                CardExpMonth = 11,
                CardExpYear = 2026,
                CardNameOnCard = "ABC",
                CardNumber = "12341232345",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = Guid.NewGuid()
            };

            var result = cardValidator.ValidateCardInfo(dataIn);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Card Number", result[0].FieldName);
        }

        [TestMethod]
        public void CardValidation_Fail_CardNumber_AlphaChar()
        {
            var dataIn = new Merch_NewPaymentRequest()
            {
                Amount = 15.23M,
                CardCVV = 123,
                CardExpMonth = 11,
                CardExpYear = 2026,
                CardNameOnCard = "ABC",
                CardNumber = "dasdasd333sd",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = Guid.NewGuid()
            };

            var result = cardValidator.ValidateCardInfo(dataIn);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Card Number", result[0].FieldName);
        }

        [TestMethod]
        public void CardValidation_Fail_CardNumber_Empty()
        {
            var dataIn = new Merch_NewPaymentRequest()
            {
                Amount = 15.23M,
                CardCVV = 123,
                CardExpMonth = 11,
                CardExpYear = 2026,
                CardNameOnCard = "ABC",
                CardNumber = "",
                CurrencyID = (int)CurrencyEnum.EUR,
                MerchantID = Guid.NewGuid()
            };

            var result = cardValidator.ValidateCardInfo(dataIn);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Card Number", result[0].FieldName);
        }

        // TODO Plenty more validation tests that could be done here on various card inputs, but I stopped here for brevity.
    }
}
