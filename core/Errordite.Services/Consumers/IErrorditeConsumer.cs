using Errordite.Core.Messages;
using Newtonsoft.Json;

namespace Errordite.Services.Consumers
{
    public interface IErrorditeConsumer
    {
        string OrganisationId { get; set; }
        void SetMessage(string body);
        void Consume();
    }

    public class ReceiveErrorConsumer : IErrorditeConsumer
    {
        private ErrorReceivedMessage _message;

        public string OrganisationId { get; set; }

        public void SetMessage(string body)
        {
            _message = JsonConvert.DeserializeObject<ErrorReceivedMessage>(body);
        }

        public void Consume()
        {
            throw new System.NotImplementedException();
        }
    }
}