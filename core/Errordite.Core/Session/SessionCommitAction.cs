

namespace Errordite.Core.Session
{
    public abstract class SessionCommitAction
    {
        protected SessionCommitAction(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
        public abstract void Execute(IAppSession session);
    }
}
