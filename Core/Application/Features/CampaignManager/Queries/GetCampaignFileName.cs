using System.Text.Json;
using Application.Common.CQS.Queries;
using Application.Common.Repositories;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CampaignManager.Queries;


public class GetCampaignFileNameResult
{
    public string? Data { get; init; }
}

public class GetCampaignFileNameRequest : IRequest<GetCampaignFileNameResult>
{
    public string? FileName {get;init;}
}

public class GetCampaignFileNameHandler : IRequestHandler<GetCampaignFileNameRequest, GetCampaignFileNameResult>
{
    private readonly IQueryContext _context;
    private readonly ICommandRepository<Campaign> _contextCampaign;
    private readonly ICommandRepository<Budget> _contextBudget;
    private readonly ICommandRepository<Expense> _contextExpense;

    public GetCampaignFileNameHandler(
        IQueryContext context,
                 ICommandRepository<Campaign> contextCampaign,
         ICommandRepository<Budget> contextBudget,
         ICommandRepository<Expense> contextExpense
        )
    {
        _contextBudget = contextBudget;
        _contextCampaign = contextCampaign;
        _contextExpense = contextExpense;
        _context = context;
    }

    public async Task<GetCampaignFileNameResult> Handle(GetCampaignFileNameRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine(request.FileName);
        using StreamReader reader = new(request.FileName);
        string text = reader.ReadToEnd();
        Campaign? campaign = JsonSerializer.Deserialize<Campaign>(text);
        campaign.Title = campaign.Title + "-copy";
        Campaign tmp = new Campaign{
            Number = campaign.Number,
            Title = campaign.Title,
            Status = campaign.Status,
            SalesTeamId = campaign.SalesTeamId,
            TargetRevenueAmount = campaign.TargetRevenueAmount,
            CampaignDateStart = campaign.CampaignDateStart,
            CampaignDateFinish = campaign.CampaignDateFinish
        };

        await _contextCampaign.CreateAsync(tmp,cancellationToken);
        List<Budget> budgets = new List<Budget>();

        foreach (var item in campaign.CampaignBudgetList)
        {   
            item.CampaignId = campaign.Id;
            item.Title += "-copy";
            item.Number += "-copy";
            budgets.Add(new Budget{
                Number = item.Number,
                Title = item.Title,
                Status = item.Status,
                Amount = item.Amount,
                CampaignId = tmp.Id,
                BudgetDate = item.BudgetDate
            });
        }

        List<Expense> expenses = new List<Expense>();
        foreach (var item in campaign.CampaignExpenseList)
        {
            item.CampaignId = campaign.Id;
            item.Title += "-copy";
            item.Number += "-copy";
            expenses.Add(new Expense{
                Number = item.Number,
                Title = item.Title,
                Status = item.Status,
                Amount = item.Amount,
                CampaignId = tmp.Id,
                ExpenseDate = item.ExpenseDate
            });
        }
        await _contextBudget.CreateListAsync(budgets , cancellationToken);
        await _contextExpense.CreateListAsync(expenses , cancellationToken);

        return new GetCampaignFileNameResult{
            Data = "Yes"
        };
    }
} 