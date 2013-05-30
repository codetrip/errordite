using Errordite.Core.Messaging;

namespace Errordite.Core.Session.Actions
{
    public class SendMessageCommitAction : SessionCommitAction
    {
        private readonly MessageBase _message;
        private readonly string _destination;

        public SendMessageCommitAction(MessageBase message, string destination)
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
