using Errordite.Core.Domain.Error;
using Raven.Abstractions.Data;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session
{
    public class DeleteAllErrorsCommitAction : DeleteByIndexCommitAction
    {
        public DeleteAllErrorsCommitAction(string issueId, bool allowStale = true)
            : base(CoreConstants.IndexNames.Errors, new IndexQuery
            {
                Query = "IssueId:{0}".FormatWith(Issue.GetId(issueId))
            }, allowStale)
        {
        }
    }

    public class DeleteAllDailyCountsCommitAction : DeleteByIndexCommitAction
    {
        public DeleteAllDailyCountsCommitAction(string issueId, bool deleteHistorical = true, bool allowStale = true)
            : base(CoreConstants.IndexNames.IssueDailyCount, new IndexQuery
            {
                Query = "IssueId:{0}{1}".FormatWith(Issue.GetId(issueId), deleteHistorical ? "" : " AND Historical:false")
            }, allowStale)
        {
        }
    }

    public abstract class DeleteByIndexCommitAction : SessionCommitAction
    {
        private readonly string _indexName;
        private readonly IndexQuery _query;
        private readonly bool _allowStale;

        protected DeleteByIndexCommitAction(string indexName, IndexQuery query, bool allowStale = false)
            : base("Delete by index")
        {
            _indexName = indexName;
            _query = query;
            _allowStale = allowStale;
        }

        public override void Execute(IAppSession session)
        {
            session.RavenDatabaseCommands.DeleteByIndex(_indexName, _query, _allowStale);
        }
    }
}
