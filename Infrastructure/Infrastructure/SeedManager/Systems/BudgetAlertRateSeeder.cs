using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Common.Repositories;
using Application.Common.CQS.Queries;
using Application.Features.NumberSequenceManager;

namespace Infrastructure.SeedManager.Systems;
public class BudgetAlertRateSeeder
{   
    private readonly ICommandRepository<BudgetAlertRate> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NumberSequenceService _numberSequenceService;
    private readonly IQueryContext _context;
    public BudgetAlertRateSeeder( 
        ICommandRepository<BudgetAlertRate> repository,
        IUnitOfWork unitOfWork,
        IQueryContext context,
        NumberSequenceService numberSequenceService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
        _numberSequenceService = numberSequenceService;
    }

    public async Task GenerateDataAsync()
    {
        if (!await _context.Set<BudgetAlertRate>().AnyAsync())
        {
            var defaultAlertRate = new BudgetAlertRate
            {
                Number = _numberSequenceService.GenerateNumber(nameof(BudgetAlertRate), "", "BAR"),
                Rate = 70.0
            };

            await _repository.CreateAsync(defaultAlertRate);
            await _unitOfWork.SaveAsync();
        }
    }
}