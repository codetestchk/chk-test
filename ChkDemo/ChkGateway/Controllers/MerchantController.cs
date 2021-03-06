using System.Threading.Tasks;
using ChkSDK.DTOs;
using ChkSDK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChkGateway.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MerchantController : ControllerBase
    {
        private readonly ILogger<MerchantController> _logger;
        private readonly IMerchantService _merchantService;

        public MerchantController(ILogger<MerchantController> logger, IMerchantService merchantService)
        {
            // TODO use logger
            _logger = logger;
            _merchantService = merchantService;
        }

        [HttpPost]
        public async Task<ActionResult<Merch_NewMerchantResponse>> PostNewMerchant(Merch_NewMerchantRequest merchantRequest)
        {
            // TODO validate input of merchantRequest

            var newMerchInfo = await _merchantService.CreateNewMerchant(merchantRequest.BankAcc, merchantRequest.BankSort, merchantRequest.Name);
            return Ok(new Merch_NewMerchantResponse()
            {
                APIKey = newMerchInfo.APIKey,
                ID = newMerchInfo.ID
            }); 
        }

        [HttpPost]
        public async Task<ActionResult<Merch_AuthenticationResponse>> Authenticate(Merch_AuthenticationRequest authRequest)
        {
            var response = await _merchantService.Authenticate(authRequest);

            if(response == null)
            {
                return Unauthorized(new { message = "authentication failed" });
            }

            return Ok(response);
        }
    }
}
