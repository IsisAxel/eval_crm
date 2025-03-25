using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ExpenseManager.Queries;

public record GetExpenseAmountListByCampaignDto
{
    public double? TotalAmount { get; init; }
    public string? CampaignName { get; init; }
}

public class GetExpenseAmountListByCampaignResult
{
    public List<GetExpenseAmountListByCampaignDto>? Data { get; init; }
}

public class GetExpenseAmountListByCampaignRequest : IRequest<GetExpenseAmountListByCampaignResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetExpenseAmountListByCampaignHandler : IRequestHandler<GetExpenseAmountListByCampaignRequest, GetExpenseAmountListByCampaignResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetExpenseAmountListByCampaignHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetExpenseAmountListByCampaignResult> Handle(GetExpenseAmountListByCampaignRequest request, CancellationToken cancellationToken)
    {
        var expensesByCampaign = await _context.Expense
            .AsNoTracking()
            .Include(e => e.Campaign)
            .IsDeletedEqualTo(request.IsDeleted)
            .Where(e => e.Campaign != null)
            .GroupBy(e => e.Campaign!.Title)
            .Select(g => new GetExpenseAmountListByCampaignDto
            {
                CampaignName = g.Key,
                TotalAmount = g.Sum(e => e.Amount ?? 0)
            })
            .ToListAsync(cancellationToken);

        return new GetExpenseAmountListByCampaignResult
        {
            Data = expensesByCampaign
        };
    }
}