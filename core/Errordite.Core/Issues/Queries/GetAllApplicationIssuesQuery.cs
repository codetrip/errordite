using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Raven.Client.Linq;

namespace Errordite.Core.Issues.Queries
{
    public class GetAllApplicationIssuesQuery : SessionAccessBase, IGetAllApplicationIssuesQuery
    {
        private readonly ErrorditeConfiguration _configuration;

        public GetAllApplicationIssuesQuery(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GetAllApplicationIssuesResponse Invoke(GetAllApplicationIssuesRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var issues = Session.Raven.Query<IssueDocument, Issues_Search>().Statistics(out stats)
                .Where(i => i.ApplicationId == Application.GetId(request.ApplicationId))
                .Skip(0)
                .Take(_configuration.MaxPageSize)
                .As<Issue>()
                .Select(issue => new IssueBase
                    {
                        Id = issue.Id,
                        Rules = issue.Rules,
                        LastErrorUtc = issue.LastErrorUtc,
                        MatchPriority = issue.MatchPriority,
                        ApplicationId = issue.ApplicationId
                    })
                .ToList();

            if(stats.TotalResults > _configuration.MaxPageSize)
            {
                int pageNumber = stats.TotalResults/_configuration.MaxPageSize;

                for(int i=1;i<pageNumber;i++)
                {
                    issues.AddRange(Session.Raven.Query<IssueDocument, Issues_Search>()
                        .Where(issue => issue.ApplicationId == Application.GetId(request.ApplicationId))
                        .Skip(pageNumber * _configuration.MaxPageSize)
                        .Take(_configuration.MaxPageSize)
                        .As<Issue>()
                        .Select(issue => new IssueBase
                        {
                            Id = issue.Id,
                            Rules = issue.Rules,
                            LastErrorUtc = issue.LastErrorUtc,
                            MatchPriority = issue.MatchPriority,
                            ApplicationId = issue.ApplicationId
                        }));
                }
            }

            return new GetAllApplicationIssuesResponse
            {
                Issues = issues
            };
        }
    }

    public interface IGetAllApplicationIssuesQuery : IQuery<GetAllApplicationIssuesRequest, GetAllApplicationIssuesResponse>
    { }

    public class GetAllApplicationIssuesResponse
    {
        public List<IssueBase> Issues { get; set; }
    }

    public class GetAllApplicationIssuesRequest
    {
        public string ApplicationId { get; set; }
    }
}
