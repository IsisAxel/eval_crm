using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BudgetManager.Queries;

public record GetBudgetByCampaignDto
{
    public string? CampaignId { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    public double TotalAmount { get; init; }
}

public class GetBudgetByCampaignResult
{
    public List<GetBudgetByCampaignDto>? Data { get; init; }
}

public class GetBudgetByCampaignRequest : IRequest<GetBudgetByCampaignResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetBudgetByCampaignHandler : IRequestHandler<GetBudgetByCampaignRequest, GetBudgetByCampaignResult>
{
    private readonly IQueryContext _context;

    public GetBudgetByCampaignHandler(IQueryContext context)
    {
        _context = context;
    }

    public async Task<GetBudgetByCampaignResult> Handle(GetBudgetByCampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await _context.Budget
            .AsNoTracking()
            .IsDeletedEqualTo(request.IsDeleted)
            .Include(x => x.Campaign)
            .GroupBy(x => new { x.CampaignId, x.Campaign.Title })
            .Select(g => new GetBudgetByCampaignDto
            {
                CampaignId = g.Key.CampaignId,
                CampaignName = g.Key.Title,
                TotalAmount = g.Sum(b => b.Amount ?? 0)
            })
            .ToListAsync(cancellationToken);

            foreach (var item in result)
            {
                Console.WriteLine($"CampaignId: {item.CampaignId}, CampaignName: {item.CampaignName}, TotalAmount: {item.TotalAmount}");
            }


        return new GetBudgetByCampaignResult
        {
            Data = result
        };
    }
}