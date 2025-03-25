using Application.Common.Services.EmailManager;
using Application.Features.BudgetManager.Commands;
using Application.Features.DashboardManager.Queries;
using Application.Features.DataManager;
using ASPNET.BackEnd.Common.Base;
using ASPNET.BackEnd.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;

[Route("api/[controller]")]
public class DataController : BaseApiController
{
    ICsvService _csvService;
    public DataController(ISender sender , ICsvService csvService) : base(sender)
    {
        _csvService = csvService;
    }


    [Authorize]
    [HttpGet("ResetData")]
    public async Task<ActionResult<ApiSuccessResult<object>>> ResetData(
        CancellationToken cancellationToken
        )
    {
        var request = new ResetDataRequest { };
        var response = await _sender.Send(request, cancellationToken);

        return Ok(new ApiSuccessResult<object>
        {
            Code = StatusCodes.Status200OK,
            Message = $"Success executing {nameof(ResetData)}",
            Content = null
        });
    }

    [Authorize]
    [HttpPost("UploadCsv")]
    public async Task<IActionResult> UploadCsv(IFormFile file1, IFormFile file2 , [FromForm]string userId , CancellationToken cancellationToken)
    {
        if (file1 == null && file2 == null)
        {
            return BadRequest(new { message = "Aucun fichier uploadé." });
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "CsvUploads");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        string filePath1 = null;
        string filePath2 = null;

        if (file1 != null)
        {
            filePath1 = Path.Combine(tempDirectory, Path.GetFileName(file1.FileName));
            using (var stream = new FileStream(filePath1, FileMode.Create))
            {
                await file1.CopyToAsync(stream);
            }
        }

        if (file2 != null)
        {
            filePath2 = Path.Combine(tempDirectory, Path.GetFileName(file2.FileName));
            using (var stream = new FileStream(filePath2, FileMode.Create))
            {
                await file2.CopyToAsync(stream);
            }
        }
        List<Campaign> campaigns = await _csvService.ImportCampaignCsv(filePath2,userId,cancellationToken);
        string error = await _csvService.ImportBudgetAndExpenseCsv(filePath1,userId,campaigns,cancellationToken);
        return Ok(new ApiSuccessResult<object>
        {
            Code = StatusCodes.Status200OK,
            Message = $"Success executing {nameof(UploadCsv)}",
            Content = error.Equals(string.Empty) ? null : error
        });
    }
}


