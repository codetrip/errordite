
using System.Collections.Generic;
using Errordite.Core.Domain.Error;
using System.Linq;

namespace Errordite.Core.Matching
{
    public interface IFindClosestMatchingIssue
    {
        Issue Find(IEnumerable<Issue> issues);
    }

    public class FindClosestMatchingIssue : IFindClosestMatchingIssue
    {
        public Issue Find(IEnumerable<Issue> issues)
        {
            var scores = issues.Select(GetScore).ToList();
            scores.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));
            return scores.Last().Issue;
        }

        private IssueScore GetScore(Issue issue)
        {
            return new IssueScore
            {
                Issue = issue,
                Score = issue.Rules.Count() + (int)issue.MatchPriority
            };
        }

        private class IssueScore
        {
            public Issue Issue { get; set; }
            public float Score { get; set; }
        }
    }
}
