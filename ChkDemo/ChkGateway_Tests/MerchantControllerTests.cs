using System;
using System.Threading.Tasks;
using ChkGateway.Controllers;
using ChkSDK.DTOs;
using ChkSDK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ChkGateway_Tests
{
    [TestClass]
    public class MerchantControllerTests
    {
        [TestMethod]
        public async Task PostNewMerchant_Success_ReturnsOK()
        {
            // Setup
            var merchantServiceMocked = new Mock<IMerchantService>();
            merchantServiceMocked.Setup(a =>
            a.CreateNewMerchant(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new NewMerchantInfo()
                {
                    APIKey = "123",
                    ID = new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838")
                });
            var merchantController = new MerchantController(null, merchantServiceMocked.Object);

            // Action
            var result = await merchantController.PostNewMerchant(new Merch_NewMerchantRequest()
            {
                BankAcc = "Anything",
                BankSort = "Anthing",
                Name = "Anything"
            });

            // Assert
            // Not a right lot to test here.. Method only returns OK as we have no validation on 'new merchant' method
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_NewMerchantResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Authenticate_Success_ReturnsOK()
        {
            // Setup
            var merchantServiceMocked = new Mock<IMerchantService>();
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.IsAny<Merch_AuthenticationRequest>()))
                .ReturnsAsync(new Merch_AuthenticationResponse()
                {
                    Token = "validToken"
                });
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.Is<Merch_AuthenticationRequest>(b => b.MerchantID == new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838") && b.MerchantAPIKey == "123")))
                .ReturnsAsync(new Merch_AuthenticationResponse()
                {
                    Token = "validToken"
                });
            var merchantController = new MerchantController(null, merchantServiceMocked.Object);

            // Action
            var result = await merchantController.Authenticate(new Merch_AuthenticationRequest()
            {
                MerchantAPIKey = "123",
                MerchantID = new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838")
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_AuthenticationResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Authenticate_Fail_WrongMerchID_ReturnsBadRequest()
        {
            // Setup
            var merchantServiceMocked = new Mock<IMerchantService>();
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.IsAny<Merch_AuthenticationRequest>()))
                .ReturnsAsync((Merch_AuthenticationResponse)null);
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.Is<Merch_AuthenticationRequest>(b => b.MerchantID == new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838") && b.MerchantAPIKey == "123")))
                .ReturnsAsync(new Merch_AuthenticationResponse()
                {
                    Token = "validToken"
                });
            var merchantController = new MerchantController(null, merchantServiceMocked.Object);

            // Action
            var result = await merchantController.Authenticate(new Merch_AuthenticationRequest()
            {
                MerchantAPIKey = "123",
                MerchantID = new Guid("21b54580-2573-4a57-81ef-a9b46af2360c") // Note, this is the 'wrong' Merchant ID ( i.e. non existant )
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_AuthenticationResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Authenticate_Fail_WrongApiKey_ReturnsBadRequest()
        {
            // Setup
            var merchantServiceMocked = new Mock<IMerchantService>();
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.IsAny<Merch_AuthenticationRequest>()))
                .ReturnsAsync((Merch_AuthenticationResponse)null);
            merchantServiceMocked.Setup(a =>
            a.Authenticate(It.Is<Merch_AuthenticationRequest>(b => b.MerchantID == new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838") && b.MerchantAPIKey == "123")))
                .ReturnsAsync(new Merch_AuthenticationResponse()
                {
                    Token = "validToken"
                });
            var merchantController = new MerchantController(null, merchantServiceMocked.Object);

            // Action
            var result = await merchantController.Authenticate(new Merch_AuthenticationRequest()
            {
                MerchantAPIKey = "WRONGKEY", // Note this is the 'wrong' api key
                MerchantID = new Guid("294c5f52-9337-414c-b8d2-aa3f49e4d838")
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<Merch_AuthenticationResponse>));
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
        }
    }
}
