using System.Linq;
using Raven.Client.Indexes;

namespace Errordite.Core.Session
{
    /// <summary>
    /// Session commit action used to ensure we dont get stale results after writing to Raven. Can be used in PRG and should be 
    /// applied to the POST action, so by the time we get to the GET we know we have up to date results as of the last write
    /// </summary>
    public class SynchroniseIndex<T> : SessionCommitAction where T : AbstractIndexCreationTask, new()
    {
        private readonly bool _centralRaven;

        public SynchroniseIndex(bool centralRaven = false) :
            base("Synchronise indexes")
        {
            _centralRaven = centralRaven;
        }

        public override void Execute(IAppSession session)
        {
            var result = session.Raven.Advanced.LuceneQuery<object>(new T().IndexName)
                .WaitForNonStaleResultsAsOfLastWrite()
                .Take(1)
                .FirstOrDefault();
        }
    }
}
