using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Messaging;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Errors.Commands
{
    public class MoveErrorsToNewIssueCommand : SessionAccessBase, IMoveErrorsToNewIssueCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public MoveErrorsToNewIssueCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MoveErrorsToNewIssueResponse Invoke(MoveErrorsToNewIssueRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            new SynchroniseIndex<Errors_Search>().Execute(Session);

            //now move errors from the other issues
            Session.RavenDatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = request.Errors.Select(e => e.Id).ToRavenQuery("Id")
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = Issue.GetId(request.IssueId)
                    }
            }, true);

            Session.AddCommitAction(new SendMessageCommitAction("Sync Issue Error Counts", new SyncIssueErrorCountsMessage
            {
                IssueId = request.IssueId,
                OrganisationId = request.CurrentUser.OrganisationId,
                TriggerEventUtc = DateTime.UtcNow,
            }, _configuration.EventsQueueName));
            
            return new MoveErrorsToNewIssueResponse();
        }
    }

    public interface IMoveErrorsToNewIssueCommand : ICommand<MoveErrorsToNewIssueRequest, MoveErrorsToNewIssueResponse>
    { }

    public class MoveErrorsToNewIssueResponse
    { }

    public class MoveErrorsToNewIssueRequest : OrganisationRequestBase
    {
        public List<Error> Errors { get; set; }
        public string IssueId { get; set; }
    }
}
