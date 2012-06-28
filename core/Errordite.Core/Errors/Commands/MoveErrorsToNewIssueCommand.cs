using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Error;
using Raven.Abstractions.Data;

namespace Errordite.Core.Errors.Commands
{
    public class MoveErrorsToNewIssueCommand : SessionAccessBase, IMoveErrorsToNewIssueCommand
    {
        public MoveErrorsToNewIssueResponse Invoke(MoveErrorsToNewIssueRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            //now move errors from the other issues
            Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = request.Errors.Select(e => e.Id).ToRavenQuery("Id")
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "Classified",
                        Type = PatchCommandType.Set,
                        Value = true
                    },
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = Issue.GetId(request.IssueId)
                    }
            }, true);
            
            return new MoveErrorsToNewIssueResponse();
        }
    }

    public interface IMoveErrorsToNewIssueCommand : ICommand<MoveErrorsToNewIssueRequest, MoveErrorsToNewIssueResponse>
    { }

    public class MoveErrorsToNewIssueResponse
    { }

    public class MoveErrorsToNewIssueRequest
    {
        public List<Error> Errors { get; set; }
        public string IssueId { get; set; }
    }
}
