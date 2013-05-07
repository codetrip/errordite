namespace Errordite.Core.Domain.Organisation
{
    public class PlanQuotas
    {
        public int IssuesExceededBy { get; set; }
        public int UsersExceededBy { get; set; }
        public int ApplicationsExceededBy { get; set; }

        public static PlanQuotas FromStats(Statistics stats, PaymentPlan plan)
        {
            return new PlanQuotas
            {
                ApplicationsExceededBy = stats.Applications - plan.MaximumApplications,
                IssuesExceededBy = stats.TotalIssues - plan.MaximumIssues,
                UsersExceededBy = stats.Users - plan.MaximumUsers
            };
        }
    }
}