using Application.Common.Repositories;
using Application.Features.NumberSequenceManager;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.BudgetAlertRateManager.Commands;
public class CreateBudgetAlertRateResult
{
    public BudgetAlertRate? Data { get; set; }
}

public class CreateBudgetAlertRateRequest : IRequest<CreateBudgetAlertRateResult>
{
    public double Rate { get; init; }
    public DateTime AlertDate { get; init; }
    public string CreatedById { get; init; } = null!;
}

public class CreateBudgetAlertRateValidator : AbstractValidator<CreateBudgetAlertRateRequest>
{
    public CreateBudgetAlertRateValidator()
    {
        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("The rate must be positive");

        RuleFor(x => x.AlertDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("The alert date cannot be in the future");

    }
}

public class CreateBudgetAlertRateHandler : IRequestHandler<CreateBudgetAlertRateRequest, CreateBudgetAlertRateResult>
{
    private readonly ICommandRepository<BudgetAlertRate> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NumberSequenceService _numberSequenceService;
    public CreateBudgetAlertRateHandler(
        ICommandRepository<BudgetAlertRate> repository,
        IUnitOfWork unitOfWork,
        NumberSequenceService numberSequenceService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<CreateBudgetAlertRateResult> Handle(CreateBudgetAlertRateRequest request, CancellationToken cancellationToken)
    {
        var entity = new BudgetAlertRate
        {
            Number = _numberSequenceService.GenerateNumber(nameof(BudgetAlertRate), "", "BAR"),
            Rate = request.Rate,
            Date = request.AlertDate,
            CreatedById = request.CreatedById
        };

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new CreateBudgetAlertRateResult { Data = entity };
    }
}