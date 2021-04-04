using ChkSDK.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChkSDK_Tests
{
    [TestClass]
    public class CardDataHelperTests
    {
        [TestMethod]
        public void CardDataHelper_HideCardNumber_1()
        {
            string rawCardNumber = "1234123412341234";

            Assert.AreEqual("************1234", CardDataHelper.HideCardNumber(rawCardNumber));
        }
    }
}
