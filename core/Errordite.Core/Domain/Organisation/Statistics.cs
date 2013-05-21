
namespace Errordite.Core.Domain.Organisation
{
    public class Statistics
    {
        public int Issues { get; set; }
		public int Unacknowledged { get; set; }
		public int FixReady { get; set; }
        public int Ignored { get; set; }
        public int Acknowledged { get; set; }
        public int Solved { get; set; }
        public int Applications { get; set; }
        public int Users { get; set; }
        public int Groups { get; set; }
        public int CurrentUserIssueCount { get; set; }

        public int TotalIssues
        {
            get
            {
				return Unacknowledged + Ignored + Acknowledged + Solved + FixReady;
            }
        }
    }
}
