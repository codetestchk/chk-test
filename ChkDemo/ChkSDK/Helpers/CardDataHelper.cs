namespace ChkSDK.Helpers
{
    public static class CardDataHelper
    {
        public static string HideCardNumber(string cardNumberRaw)
        {
            // I assume all card numbers are 16 digits, and we want to only show last 4?
            return "************" + cardNumberRaw.Substring(12);
        }
    }
}
