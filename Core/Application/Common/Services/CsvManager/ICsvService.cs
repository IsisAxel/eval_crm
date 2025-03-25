using Domain.Entities;

namespace Application.Common.Services.EmailManager;

public interface ICsvService
{
    Task<List<Campaign>> ImportCampaignCsv(string filePath , string userId , CancellationToken cancellationToken);
    Task<string> ImportBudgetAndExpenseCsv(string filePath , string userId , List<Campaign> campaigns , CancellationToken cancellationToken);
}
