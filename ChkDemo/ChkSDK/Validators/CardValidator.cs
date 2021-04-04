using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChkSDK.DTOs;

namespace ChkSDK.Validators
{
    public static class CardValidator
    {
        /// <summary>
        /// Validates all of the basic things with a card, number, expiry, cvv etc, used to validate before we send to bank
        /// </summary>
        /// <param name="request">All of the card info</param>
        /// <returns>Returns list of validation failures, count = 0 if no errors</returns>
        public static List<CardValidationError> ValidateCardInfo(Merch_NewPaymentRequest request)
        {
            List<CardValidationError> toReturn = new List<CardValidationError>();

            var cardNoValidationResult = ValidateCardNumber(request.CardNumber);
            if(cardNoValidationResult != null)
            {
                toReturn.Add(cardNoValidationResult);
            }

            // TODO all the other validators would be put here that we can do on the details before posting to the bank

            return toReturn;
        }

        private static CardValidationError ValidateCardNumber(string cardNumber)
        {
            // This is avery simple Regex, there are probably better validations you can do on cards than this
            // For demo purposes, this is just a 16 digit number validator.
            var match = Regex.Match(cardNumber, @"^\d{16}$", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                // does not match
                return new CardValidationError()
                {
                    FieldName = "Card Number",
                    Advice = "Make sure it's a 16 digit card number only",
                    ValidationFailureMessage = "Invalid card number, not 16 digits"
                };
            }
            return null;
        }
    }

    public class CardValidationError
    {
        public string FieldName { get; set; }
        public string ValidationFailureMessage { get; set; }
        public string Advice { get; set; }
    }
}
