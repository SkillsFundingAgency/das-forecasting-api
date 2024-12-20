﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Api.Controllers;

[Route("api/accounts/{accountId}/[controller]/")]
public class AccountProjectionController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Route("expiring-funds")]
    public async Task<IActionResult> GetAccountExpiredFunds(long accountId)
    {
        try
        {
            var response = await mediator.Send(new GetAccountExpiringFundsQuery { AccountId = accountId });

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


    [HttpGet]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Route("projected-summary")]
    public async Task<IActionResult> GetProjectedFundingSummary(long accountId, int numberOfMonths = 12)
    {
        try
        {
            var response = await mediator.Send(new GetAccountProjectionSummaryQuery { AccountId = accountId, NumberOfMonths = numberOfMonths });

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

    [HttpGet]
    [Route("detail")]
    public async Task<IActionResult> GetProjectedFundingDetail(long accountId, DateTime startDate, int numberOfMonths)
    {
        try
        {
            var query = new GetAccountProjectionDetailQuery
            {
                AccountId = accountId,
                StartDate = startDate,
                NumberOfMonths = numberOfMonths
            };

            var response = await mediator.Send(query);

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