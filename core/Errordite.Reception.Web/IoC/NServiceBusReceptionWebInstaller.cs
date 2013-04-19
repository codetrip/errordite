
using Errordite.Core.ServiceBus;
using Errordite.Core.Messages;

namespace Errordite.Reception.Web.IoC
{
    public class NServiceBusReceptionWebInstaller : NServiceBusClientInstaller
    {
        public NServiceBusReceptionWebInstaller()
            : base(new[]
                {
                    typeof (ReceiveErrorMessage).Assembly
                })
        { }
    }
}