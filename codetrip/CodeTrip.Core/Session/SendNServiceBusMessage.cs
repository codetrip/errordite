using NServiceBus;

namespace CodeTrip.Core.Session
{
    public class SendNServiceBusMessage : SessionCommitAction
    {
        private readonly IMessage _message;
        private readonly string _destination;

        public SendNServiceBusMessage(string description, IMessage message, string destination)
            : base(description)
        {
            _destination = destination;
            _message = message;
        }

        public SendNServiceBusMessage(string description, IMessage message)
            : base(description)
        {
            _message = message;
        }

        public override void Execute(IAppSession session)
        {
            if (_destination == null)
                session.Bus.Send(_message);
            else
                session.Bus.Send(_destination, _message);
        }
    }
}
