using System.Text.Json;
using Application.Common.CQS.Queries;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CampaignManager.Queries;

public class GetCampaignToDuplicateResult
{
    public Campaign? Data { get; init; }
}

public class GetCampaignToDuplicateRequest : IRequest<GetCampaignToDuplicateResult>
{
    public string? Id { get; init; }
}

public class GetCampaignToDuplicateValidator : AbstractValidator<GetCampaignToDuplicateRequest>
{
    public GetCampaignToDuplicateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetCampaignToDuplicateHandler : IRequestHandler<GetCampaignToDuplicateRequest, GetCampaignToDuplicateResult>
{
    private readonly IQueryContext _context;

    public GetCampaignToDuplicateHandler(
        IQueryContext context
        )
    {
        _context = context;
    }

    public async Task<GetCampaignToDuplicateResult> Handle(GetCampaignToDuplicateRequest request, CancellationToken cancellationToken)
    {
        var query = _context
            .Campaign
            .AsNoTracking()
            .Include(x => x.CampaignBudgetList.Where(budget => !budget.IsDeleted))
            .Include(x => x.CampaignExpenseList.Where(expense => !expense.IsDeleted))
            .Include(x => x.CampaignLeadList.Where(lead => !lead.IsDeleted))
            .Where(x => x.Id == request.Id);

        Campaign? entity = await query.SingleOrDefaultAsync(cancellationToken);

        // Nettoyage des références circulaires
        foreach (var item in entity.CampaignBudgetList)
        {
            item.Campaign = null;
        }
        foreach (var item in entity.CampaignExpenseList)
        {
            item.Campaign = null;
        }

        return new GetCampaignToDuplicateResult
        {
            Data = entity
        };
    }
}