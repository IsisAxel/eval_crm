using Application.Features.BudgetManager.Commands;
using Application.Features.DashboardManager.Queries;
using Application.Features.DataManager;
using ASPNET.BackEnd.Common.Base;
using ASPNET.BackEnd.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;

[Route("api/[controller]")]
public class DataController : BaseApiController
{
    public DataController(ISender sender) : base(sender)
    {
    }


    [Authorize]
    [HttpGet("ResetData")]
    public async Task<ActionResult<ApiSuccessResult<object>>> GetCRMDashboardAsync(
        CancellationToken cancellationToken
        )
    {
        var request = new ResetDataRequest { };
        var response = await _sender.Send(request, cancellationToken);

        return Ok(new ApiSuccessResult<object>
        {
            Code = StatusCodes.Status200OK,
            Message = $"Success executing {nameof(GetCRMDashboardAsync)}",
            Content = null
        });
    }

}


