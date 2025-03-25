using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CampaignManager.Queries;

public record StatusRevenueSummaryDto
{
    public string? CampaignStatus { get; init; }
    public double TotalRevenueTarget { get; init; }
}

public class CampaignRevenueSummaryProfile : Profile
{
    public CampaignRevenueSummaryProfile()
    {
        CreateMap<Campaign, StatusRevenueSummaryDto>()
            .ForMember(dest => dest.CampaignStatus, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalRevenueTarget, opt => opt.MapFrom(src => src.TargetRevenueAmount ?? 0));
    }
}

public class GetCampaignRevenueSummaryResult
{
    public List<StatusRevenueSummaryDto>? Data { get; init; }
    // public double GrandTotalRevenue { get; init; }
}

public class GetCampaignRevenueSummaryRequest : IRequest<GetCampaignRevenueSummaryResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetCampaignRevenueSummaryHandler : IRequestHandler<GetCampaignRevenueSummaryRequest, GetCampaignRevenueSummaryResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetCampaignRevenueSummaryHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetCampaignRevenueSummaryResult> Handle(GetCampaignRevenueSummaryRequest request, CancellationToken cancellationToken)
    {
        var campaigns = await _context.Campaign
            .AsNoTracking()
            .IsDeletedEqualTo(request.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<StatusRevenueSummaryDto>>(campaigns);
        // var grandTotal = dtos.Sum(x => x.TotalRevenueTarget);

        // Group by status and calculate totals
        var statusTotals = dtos
            .GroupBy(c => c.CampaignStatus ?? "Unknown")
            .Select(g => new StatusRevenueSummaryDto
            {
                CampaignStatus = g.Key,
                TotalRevenueTarget = g.Sum(c => c.TotalRevenueTarget)
            })
            .ToList();

        return new GetCampaignRevenueSummaryResult
        {
            Data = statusTotals,
            // GrandTotalRevenue = grandTotal
        };
    }
}