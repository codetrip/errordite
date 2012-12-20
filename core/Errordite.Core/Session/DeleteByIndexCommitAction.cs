using Raven.Abstractions.Data;

namespace Errordite.Core.Session
{
    public class DeleteByIndexCommitAction : SessionCommitAction
    {
        private readonly string _indexName;
        private readonly IndexQuery _query;
        private readonly bool _allowStale;

        public DeleteByIndexCommitAction(string indexName, IndexQuery query, bool allowStale = false)
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
