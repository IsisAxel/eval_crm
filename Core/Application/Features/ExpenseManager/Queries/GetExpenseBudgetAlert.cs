using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using Application.Features.BudgetAlertRateManager.Queries;
using Application.Features.CampaignManager.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ExpenseManager.Queries;

public class GetExpenseBudgetAlertResult
{
    public string? Data { get; init; }
}

public class GetExpenseBudgetAlertRequest : IRequest<GetExpenseBudgetAlertResult>
{
    public double Amount {get; init;}
    public string? CampaignId {get ; init;}
    public DateTime? ExpenseDate {get; init;}
}

public class GetExpenseBudgetAlertHandler : IRequestHandler<GetExpenseBudgetAlertRequest, GetExpenseBudgetAlertResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;
    private readonly ISender _sender;

    public GetExpenseBudgetAlertHandler(IMapper mapper, IQueryContext context , ISender sender)
    {
        _mapper = mapper;
        _context = context;
        _sender = sender;
    }

    public async Task<GetExpenseBudgetAlertResult> Handle(GetExpenseBudgetAlertRequest request, CancellationToken cancellationToken)
    {
        var campaignResult = await _sender.Send(
            new GetCampaignSingleRequest { Id = request.CampaignId }, 
            cancellationToken);
        
        var campaign = campaignResult.Data;

        var alertRateResult = await _sender.Send(
            new GetLastBudgetAlertRateByDateRequest { EndDate = request.ExpenseDate.GetValueOrDefault() },
            cancellationToken);

        var alertRate = alertRateResult.Data;
        Console.WriteLine("ALERT : "+alertRate == null);
        Console.WriteLine("CAMPAIGN : "+ campaign == null);

        var alertMessage = campaign!.checkBudgetAlertRate(request.Amount , request.ExpenseDate.GetValueOrDefault(), alertRate!);      
        return new GetExpenseBudgetAlertResult
        {
            Data = alertMessage
        };
    }
}