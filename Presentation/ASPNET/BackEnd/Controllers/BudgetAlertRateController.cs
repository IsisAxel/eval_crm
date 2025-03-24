using Application.Features.BudgetAlertRateManager.Commands;
using Application.Features.BudgetAlertRateManager.Queries;
using Application.Features.ExpenseManager.Queries;
using ASPNET.BackEnd.Common.Base;
using ASPNET.BackEnd.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;

[Route("api/[controller]")]
public class BudgetAlertRateController : BaseApiController
{
    public BudgetAlertRateController(ISender sender) : base(sender)
    {
    }

    [Authorize]
    [HttpPost("CreateBudgetAlertRate")]
    public async Task<ActionResult<ApiSuccessResult<CreateBudgetAlertRateResult>>> CreateBudgetAlertRateAsync(CreateBudgetAlertRateRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);

        return Ok(new ApiSuccessResult<CreateBudgetAlertRateResult>
        {
            Code = StatusCodes.Status200OK,
            Message = $"Success executing {nameof(CreateBudgetAlertRateAsync)}",
            Content = response
        });
    }

    [Authorize]
    [HttpPost("CheckBudgetAlertRate")]
    public async Task<ActionResult<ApiSuccessResult<GetExpenseBudgetAlertResult>>> CheckBudgetAlertRateAsync(GetExpenseBudgetAlertRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);

        return Ok(new ApiSuccessResult<GetExpenseBudgetAlertResult>
        {
            Code = StatusCodes.Status200OK,
            Message = $"Success executing {nameof(CheckBudgetAlertRateAsync)}",
            Content = response
        });
    }
}


