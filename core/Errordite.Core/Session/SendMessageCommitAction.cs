using Errordite.Core.Messaging;

namespace Errordite.Core.Session
{
    public class SendMessageCommitAction : SessionCommitAction
    {
        private readonly MessageBase _message;
        private readonly string _destination;

        public SendMessageCommitAction(string description, MessageBase message, string destination)
            : base(description)
        {
            _destination = destination;
            _message = message;
        }

        public override void Execute(IAppSession session)
        {
            session.MessageSender.Send(_message, _destination);
        }
    }
}
