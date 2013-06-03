namespace Errordite.Core.Domain.Organisation
{
    public class PlanQuotas
    {
        public int IssuesExceededBy { get; set; }

        public static PlanQuotas FromStats(Statistics stats, PaymentPlan plan)
        {
            return new PlanQuotas
            {
                IssuesExceededBy = stats.TotalIssues - plan.MaximumIssues,
            };
        }
    }
}