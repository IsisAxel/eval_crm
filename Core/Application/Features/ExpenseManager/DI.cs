using System;
using Application.Features.BudgetAlertRateManager.Queries;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.ExpenseManager;

public static class DI
{
    public static string checkBudgetAlertRate(this Campaign campaign, double amount , DateTime endTime ,BudgetAlertRate alertRate)
    {
         if (campaign == null || !campaign.CampaignBudgetList.Any())
        {
            return string.Empty;
        }

        var existingExpenses = campaign.CampaignExpenseList
            .Where(e => e.ExpenseDate <= endTime && (e.Status == ExpenseStatus.Confirmed || e.Status == ExpenseStatus.Archived))
            .Sum(e => e.Amount.GetValueOrDefault());

        var totalExpenses = existingExpenses + amount;

        var campaignBudget = campaign.CampaignBudgetList
            .Where(b => b.BudgetDate <= endTime && (b.Status == BudgetStatus.Confirmed || b.Status == BudgetStatus.Archived))
            .Sum(b => b.Amount.GetValueOrDefault());

        var isAlert = false;
        var alertMessage = string.Empty;

        Console.WriteLine($"Campaign Budget : {campaignBudget}");
        Console.WriteLine($"Total Expenses : {totalExpenses}");
        if (alertRate != null)
        {
            Console.WriteLine($"Alert Rate : {alertRate.Rate}");
            var alertThreshold = campaignBudget * alertRate.Rate/100;
            isAlert = totalExpenses >= alertThreshold;
            
            if (isAlert)
            {
                alertMessage = $"Total expenses '{totalExpenses}' " +
                            $"are more than {alertRate.Rate}% " +
                            $"of the budget ' {campaignBudget} ' of the Campaign '{campaign.Title}'.";
            }
        }
        return alertMessage;
    }   
}