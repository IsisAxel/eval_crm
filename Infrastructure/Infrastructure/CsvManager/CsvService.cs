using System.Globalization;
using System.Text;
using Application.Common.Repositories;
using Application.Common.Services.EmailManager;
using Application.Features.NumberSequenceManager;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;

namespace Infrastructure.CsvManager
{
    public class CsvService : ICsvService
    {
        private readonly ICommandRepository<Campaign> _contextCampaign;
        private readonly ICommandRepository<Budget> _contextBudget;
        private readonly ICommandRepository<Expense> _contextExpense;
        private readonly NumberSequenceService _numberSequenceService;

        public CsvService(ICommandRepository<Campaign> contextCampaign , ICommandRepository<Budget> contextBudget, ICommandRepository<Expense> contextExpense , NumberSequenceService numberSequenceService){
            _contextCampaign = contextCampaign;
            _contextBudget = contextBudget;
            _contextExpense = contextExpense;
            _numberSequenceService = numberSequenceService;
        }
        public List<Dictionary<string, object>> CsvToObject(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,  // Ignore la première ligne (en-têtes)
                Delimiter = ",",         // Délimiteur virgule
                BadDataFound = null      // Ignore les données corrompues
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader(); // Lire les en-têtes
                var records = new List<Dictionary<string, object>>();

                while (csv.Read())
                {
                    var row = new Dictionary<string, object>();
                    foreach (var header in csv.HeaderRecord)
                    {
                        row[header] = csv.GetField(header); // Stocker chaque valeur sous son en-tête
                    }
                    records.Add(row);
                }

                return records;
            }
        } 

        public async Task<List<Campaign>> ImportCampaignCsv(string filePath , string userId , CancellationToken cancellationToken)
        {
            try
            {
                List<Campaign> entities = new List<Campaign>();
                List<Dictionary<string, object>> csvObject = CsvToObject(filePath);

                var headers = csvObject.FirstOrDefault()?.Keys;
                if (headers == null || !headers.Contains("campaign_code") || !headers.Contains("campaign_title"))
                {
                    return null;
                }

                foreach (var item in csvObject)
                {
                    entities.Add(new Campaign{
                        Number = item["campaign_code"].ToString(),
                        Title = item["campaign_title"].ToString(),
                        CreatedById = userId,
                        Status = Domain.Enums.CampaignStatus.Confirmed,
                        TargetRevenueAmount = 100000,
                        CampaignDateStart = new DateTime(1990, 1, 1),
                        CampaignDateFinish = new DateTime(2028, 12, 31)
                    });
                }
                await _contextCampaign.CreateListAsync(entities,cancellationToken);
                return entities;
            }
            catch (Exception)
            {
                throw new Exception("Invalid csv");
            }
        }

        public async Task<string> ImportBudgetAndExpenseCsv(string filePath, string userId, List<Campaign> campaigns, CancellationToken cancellationToken)
        {
            var errorMessages = new StringBuilder();
            bool hasErrors = false;
            try
            {
                List<Budget> budgets = new List<Budget>();
                List<Expense> expenses = new List<Expense>();
                List<Dictionary<string, object>> csvObject = CsvToObject(filePath);

                // Vérification des en-têtes nécessaires
                var headers = csvObject.FirstOrDefault()?.Keys;
                if (headers == null || !headers.Contains("Campaign_number") || !headers.Contains("Title") || !headers.Contains("Type") || !headers.Contains("Date") || !headers.Contains("Amount"))
                {
                    if (campaigns == null)
                    {
                        return "Invalid header for the campaign and budget/expense csv.";
                    }
                    return "Invalid header for the budget/expense csv.";
                }
                if (campaigns == null)
                {
                    return "Invalid header for the campaign  csv.";
                }
                string[] formats = {
                    "dd/MM/yyyy",
                    "dd/MM/yyyy HH:mm:ss",
                    "dd/MM/yyyy hh:mm:ss tt"
                };

                for (int i = 0; i < csvObject.Count; i++)
                {
                    var item = csvObject[i];
                    DateTime dateValue;
                    bool isValidDate = DateTime.TryParseExact(item["Date"].ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);
                    double amount = 0;
                    StringBuilder lineErrors = new StringBuilder(); // Pour stocker les erreurs de chaque ligne

                    try
                    {
                        amount = double.Parse(item["Amount"].ToString());
                    }
                    catch (System.Exception)
                    {
                        hasErrors = true;
                        lineErrors.Append("Amount should be a number. ");
                    }

                    if (!isValidDate)
                    {
                        lineErrors.Append("Invalid date format. ");
                        hasErrors = true;
                    }

                    if (amount < 0)
                    {
                        lineErrors.Append("Negative amount. ");
                        hasErrors = true;
                    }

                    if (lineErrors.Length > 0) // Si des erreurs existent pour cette ligne
                    {
                        errorMessages.AppendLine($"Erreur à la ligne {i + 1}: {lineErrors.ToString().Trim()}\n");
                    }

                    string type = item["Type"].ToString();

                    if (type.Equals("Budget"))
                    {
                        Campaign cmp = campaigns.FirstOrDefault(entity => entity.Number.Equals(item["Campaign_number"].ToString()));
                        budgets.Add(new Budget{
                            Number = _numberSequenceService.GenerateNumber(nameof(Budget), "", "BAR"),
                            Title = item["Title"].ToString(),
                            CreatedById = userId,
                            Amount = amount,
                            Campaign = cmp,
                            CampaignId = cmp?.Id,
                            Status = Domain.Enums.BudgetStatus.Confirmed,
                            BudgetDate = dateValue
                        });
                    }
                    else if (type.Equals("Expense"))
                    {
                        Campaign cmp = campaigns.FirstOrDefault(entity => entity.Number.Equals(item["Campaign_number"].ToString()));
                        expenses.Add(new Expense{
                            Number = _numberSequenceService.GenerateNumber(nameof(Expense), "", "BAR"),
                            Title = item["Title"].ToString(),
                            CreatedById = userId,
                            Amount = amount,
                            Campaign = cmp,
                            CampaignId = cmp?.Id,
                            Status = Domain.Enums.ExpenseStatus.Confirmed,
                            ExpenseDate = dateValue
                        });
                    }
                    else
                    {
                        errorMessages.AppendLine($"Erreur à la ligne {i + 1}: Unknown type.\n");
                        hasErrors = true;
                        break; // Si le type est inconnu, on arrête l'import
                    }
                }

                if (hasErrors)
                {
                    await _contextCampaign.DeleteListAsync(campaigns, cancellationToken);
                    return errorMessages.ToString();
                }

                // Si aucune erreur, insérer les données
                await _contextBudget.CreateListAsync(budgets, cancellationToken);
                await _contextExpense.CreateListAsync(expenses, cancellationToken);

                return string.Empty;
            }
            catch (Exception ex)
            {
                // Si une exception se produit, retourner un message d'erreur générique
                return $"Une erreur est survenue : {ex.Message}";
            }
        }

    }
}