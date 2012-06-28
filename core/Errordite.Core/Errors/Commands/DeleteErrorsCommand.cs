using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Raven.Abstractions.Data;
using System.Linq;

namespace Errordite.Core.Errors.Commands
{
    public class DeleteErrorsCommand : SessionAccessBase, IDeleteErrorsCommand
    {
        public DeleteErrorsResponse Invoke(DeleteErrorsRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            foreach (var partition in request.IssueIds.Select(Issue.GetId).Partition(25))
            {
                string issueIdQuery = "({0})".FormatWith(partition.ToRavenQuery("IssueId"));

                Trace("Issuing delete query for issue ids: {0}", issueIdQuery);

                Session.Raven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.Errors, new IndexQuery
                {
                    Query = issueIdQuery
                });

                Session.Raven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.UnloggedErrors, new IndexQuery
                {
                    Query = issueIdQuery
                });
            }

            return new DeleteErrorsResponse();
        }
    }

    public interface IDeleteErrorsCommand : ICommand<DeleteErrorsRequest, DeleteErrorsResponse>
    { }

    public class DeleteErrorsResponse
    { }

    public class DeleteErrorsRequest : OrganisationRequestBase
    {
        public IEnumerable<string> IssueIds { get; set; }
    }
}
