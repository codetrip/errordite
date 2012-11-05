using NServiceBus;
using NServiceBus.Unicast.Config;

namespace CodeTrip.Core.ServiceBus
{
    public static class NServiceBusConfigurationExtensions
    {
        public static ConfigUnicastBus ConditionalLoadMessageHandlers(this ConfigUnicastBus config, bool loadMessageHandlers)
        {
            if (loadMessageHandlers)
                config.LoadMessageHandlers();

            return config;
        }

        public static IBus StartFor(this Configure config, bool sendOnly)
        {
            if (sendOnly)
                config.SendOnly();

            return config.CreateBus().Start();
        }
    }
}
