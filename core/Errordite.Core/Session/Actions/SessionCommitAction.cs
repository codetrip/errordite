

namespace Errordite.Core.Session.Actions
{
    public abstract class SessionCommitAction
    {
        public abstract void Execute(IAppSession session);
    }
}
