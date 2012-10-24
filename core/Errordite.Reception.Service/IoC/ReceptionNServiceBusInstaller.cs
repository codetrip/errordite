
using CodeTrip.Core.ServiceBus;
using Errordite.Core.Messages;
using Errordite.Reception.Service.Handlers;

namespace Errordite.Reception.Service.IoC
{
    public class ReceptionNServiceBusInstaller : NServiceBusServerInstaller
    {
        public ReceptionNServiceBusInstaller()
            : base(new[]
                {
                    typeof (ReceiveErrorMessage).Assembly,
                    typeof (ReceiveErrorHandler).Assembly
                })
        { }
    }
}
