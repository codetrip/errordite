using Castle.Core;
using ChargifyNET;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class  CreateInvoiceCommand : SessionAccessBase, ICreateInvoiceCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public CreateInvoiceCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CreateInvoiceResponse Invoke(CreateInvoiceRequest request)
        {
            Trace("Starting...");
  
            var connection = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
            var transaction = connection.LoadTransaction(request.SubscriptionId);

			if (transaction == null)
            {
                return new CreateInvoiceResponse
                {
                    Status = CreateInvoiceStatus.StatementNotFound
                };
            }

            //transaction.

            return new CreateInvoiceResponse
            {
                Status = CreateInvoiceStatus.Ok
            };
        }
    }

    public interface ICreateInvoiceCommand : ICommand<CreateInvoiceRequest, CreateInvoiceResponse>
    { }

    public class CreateInvoiceResponse
    {
        public CreateInvoiceStatus Status { get; set; }
    }

    public class CreateInvoiceRequest : OrganisationRequestBase
    {
        public int SubscriptionId { get; set; }
        public string Reference { get; set; }
    }

    public enum CreateInvoiceStatus
    {
        Ok,
        InvalidOrganisation,
        OrganisationNotFound,
        StatementNotFound
    }
}
