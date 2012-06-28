
namespace Errordite.Core.Domain.Organisation
{
    public class Statistics
    {
        public int Issues { get; set; }
        public int Unacknowledged { get; set; }
        public int Ignorable { get; set; }
        public int Acknowledged { get; set; }
        public int Solved { get; set; }
        public int Investigating { get; set; }
        public int AwaitingDeployment { get; set; }
        public int Applications { get; set; }
        public int Users { get; set; }
        public int Groups { get; set; }
        public int CurrentUserIssueCount { get; set; }

        public int TotalIssues
        {
            get
            {
                return Unacknowledged + Ignorable + Acknowledged + Solved + Investigating + AwaitingDeployment;
            }
        }

        public int ActiveIssues
        {
            get
            {
                return Unacknowledged + Ignorable + Acknowledged + Investigating + AwaitingDeployment;
            }
        }
    }
}
