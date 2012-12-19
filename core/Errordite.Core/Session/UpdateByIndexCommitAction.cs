using Raven.Abstractions.Data;

namespace Errordite.Core.Session
{
    public class UpdateByIndexCommitAction : SessionCommitAction
    {
        private readonly string _indexName;
        private readonly IndexQuery _query;
        private readonly PatchRequest[] _patchRequests;
        private readonly bool _allowStale;

        public UpdateByIndexCommitAction(string indexName, IndexQuery query, PatchRequest[] patchRequests, bool allowStale = false)
            : base("Update by index")
        {
            _indexName = indexName;
            _query = query;
            _patchRequests = patchRequests;
            _allowStale = allowStale;
        }

        public override void Execute(IAppSession session)
        {
            session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(_indexName, _query, _patchRequests, _allowStale);
        }
    }
}
