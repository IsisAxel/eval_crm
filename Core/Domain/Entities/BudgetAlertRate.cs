using Domain.Common;

namespace Domain.Entities
{
    public class BudgetAlertRate : BaseEntity
    {
        public string? Number { get; set; }
        public double Rate { get; set; }
        public DateTime Date { get; set; }
    }
}