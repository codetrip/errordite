using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Queries
{
    /// <summary>
    /// Retrieve all unclassified errors which match the rules of the incoming issue
    /// </summary>
    public class GetUnclassifiedErrorsThatMatchRulesQuery : SessionAccessBase, IGetUnclassifiedErrorsThatMatchRulesQuery
    {
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public GetUnclassifiedErrorsThatMatchRulesQuery(IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            ErrorditeConfiguration configuration)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _configuration = configuration;
        }

        public GetUnclassifiedErrorsThatMatchRulesResponse Invoke(GetUnclassifiedErrorsThatMatchRulesRequest request)
        {
            Trace("Starting...");
            Trace("Attempting to load issue with Id {0}...", request.IssueId);

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if (issue == null)
            {
                Trace("Issue with Id {0} is null cannot proceed", request.IssueId);
                return new GetUnclassifiedErrorsThatMatchRulesResponse();
            }

            Trace("Located existing issue, issue Id:={0}...", issue.Id);

            var errorsToAttach = new List<Error>();
            var pagingStatus = ProcessBatch(issue, 1, errorsToAttach);

            if (pagingStatus.TotalItems > _configuration.MaxPageSize)
            {
                ProcessBatches(issue, pagingStatus, errorsToAttach);
            }

            Trace("Located {0} errors to move to issue...", errorsToAttach.Count);

            return new GetUnclassifiedErrorsThatMatchRulesResponse
            {
                Errors = errorsToAttach
            };
        }

        private void ProcessBatches(Issue issue, PagingStatus pagingStatus, List<Error> errorsToAttach)
        {
            for (int pageNumber = 2; pageNumber <= pagingStatus.TotalPages; pageNumber++)
            {
                ProcessBatch(issue, pageNumber, errorsToAttach);
            }
        }

        private PagingStatus ProcessBatch(Issue issue, int pageNumber, List<Error> errorsToAttach)
        {
            var batch = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
            {
                ApplicationId = issue.ApplicationId,
                OrganisationId = issue.OrganisationId,
                Paging = new PageRequestWithSort(pageNumber, _configuration.MaxPageSize, CoreConstants.SortFields.TimestampUtc, false),
                Classified = false
            }).Errors;

            if (batch.Items != null && batch.Items.Count > 0)
            {
                foreach (var error in batch.Items)
                {
                    if(issue.Rules.All(rule => rule.IsMatch(error)))
                    {
                        errorsToAttach.Add(new Error { Id = error.Id, IssueId = error.IssueId });
                    }

                    Session.Raven.Advanced.Evict(error);
                }
            }

            return batch.PagingStatus;
        }
    }

    public interface IGetUnclassifiedErrorsThatMatchRulesQuery : ICommand<GetUnclassifiedErrorsThatMatchRulesRequest, GetUnclassifiedErrorsThatMatchRulesResponse>
    { }

    public class GetUnclassifiedErrorsThatMatchRulesResponse
    {
        public List<Error> Errors { get; set; }
    }

    public class GetUnclassifiedErrorsThatMatchRulesRequest
    {
        public string IssueId { get; set; }
    }
}
