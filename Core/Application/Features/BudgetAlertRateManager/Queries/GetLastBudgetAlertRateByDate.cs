using Application.Common.CQS.Queries;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BudgetAlertRateManager.Queries;

public class GetLastBudgetAlertRateByDateResult
{
    public BudgetAlertRate? Data { get; init; }
}

public class GetLastBudgetAlertRateByDateRequest : IRequest<GetLastBudgetAlertRateByDateResult>
{
    public DateTime EndDate { get; init; }
}

public class GetLastBudgetAlertRateByDateValidator : AbstractValidator<GetLastBudgetAlertRateByDateRequest>
{
    public GetLastBudgetAlertRateByDateValidator()
    {
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("La date de fin est obligatoire");
    }
}

public class GetLastBudgetAlertRateByDateHandler : IRequestHandler<GetLastBudgetAlertRateByDateRequest, GetLastBudgetAlertRateByDateResult>
{
    private readonly IQueryContext _context;

    public GetLastBudgetAlertRateByDateHandler(IQueryContext context)
    {
        _context = context;
    }

    public async Task<GetLastBudgetAlertRateByDateResult> Handle(
        GetLastBudgetAlertRateByDateRequest request,
        CancellationToken cancellationToken)
    {
        var query = _context.BudgetAlertRate
            .AsNoTracking()
            .Where(x => x.Date <= request.EndDate);

        var entity = await query
            .OrderByDescending(x => x.Date)          
            .FirstOrDefaultAsync(cancellationToken);

        return new GetLastBudgetAlertRateByDateResult { Data = entity };
    }
}