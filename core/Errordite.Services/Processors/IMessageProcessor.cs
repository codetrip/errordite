using Errordite.Core.Configuration;
using Errordite.Core.Messaging;

namespace Errordite.Services.Processors
{
    public interface IMessageProcessor
    {
        void Process(ServiceConfiguration configuration, MessageEnvelope envelope);
    }
}