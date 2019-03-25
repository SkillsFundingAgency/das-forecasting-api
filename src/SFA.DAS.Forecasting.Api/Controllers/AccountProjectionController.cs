using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Api.Controllers
{
    [Route("api/accounts/{accountId}/[controller]/")]
    public class AccountProjectionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountProjectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Route("expiring-funds")]
        public async Task<IActionResult> GetAccountExpiredFunds(long accountId)
        {
            try
            {
                var response = await _mediator.Send(new GetAccountExpiringFundsQuery { AccountId = accountId });

                if (response == null)
                {
                    return NotFound();
                }

                return Ok(response);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ArgumentErrorViewModel
                {
                    Message = e.Message,
                    Params = e.ParamName
                });
            }
        }
    }
}
