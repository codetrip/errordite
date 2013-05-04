using System;
using System.Web.Mvc;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Configuration;
using Errordite.Core.Indexing;
using Errordite.Core.Messaging;
using Errordite.Core.Monitoring.Queries;
using Errordite.Core.Paging;
using Errordite.Web.ActionFilters;
using Errordite.Web.Areas.System.Models.Monitoring;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;
using System.Linq;
using Newtonsoft.Json;
using Raven.Abstractions.Data;
using Errordite.Core.Extensions;

namespace Errordite.Web.Areas.System.Controllers
{
    public class MonitoringController : ErrorditeController
    {
	    private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
	    private readonly IGetMessageEnvelopes _getMessageEnvelopes;
        private readonly AmazonSQS _amazonSQS;

	    public MonitoringController(IGetMessageEnvelopes getMessageEnvelopes, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            AmazonSQS amazonSqs)
	    {
		    _getMessageEnvelopes = getMessageEnvelopes;
		    _pagingViewModelGenerator = pagingViewModelGenerator;
	        _amazonSQS = amazonSqs;
	    }

	    [PagingView, ImportViewData]
	    public ActionResult Index(MessageFailuresPostModel model)
        {
			var pagingRequest = GetSinglePagingRequest();
			var failures = _getMessageEnvelopes.Invoke(new GetMessageEnvelopesRequest
			{
				OrganisationId = model.OrganisationId,
				Service = model.Service,
				Paging = pagingRequest
			});

		    var viewModel = new MessageFailuresViewModel
			{
				Envelopes = failures.Envelopes.Items,
				OrganisationId = model.OrganisationId,
				Service = model.Service,
				Services = Service.Events.EnumToSelectList("Service"),
				Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, failures.Envelopes.PagingStatus, pagingRequest)
			};

			return View(viewModel);
        }

        [HttpPost, ExportViewData]
		public ActionResult Delete(MessageFailuresActionPostModel model)
        {
            if (model.EnvelopeIds == null || !model.EnvelopeIds.Any())
            {
                ErrorNotification("Please select messages to delete");
                return RedirectToAction("index", new {OrganisationId = model.OrgId, Service = model.Svc});
            }

            foreach (var envelopeId in model.EnvelopeIds)
            {
                Core.Session.MasterRaven.Advanced.DocumentStore.DatabaseCommands.Delete(MessageEnvelope.GetId(envelopeId), null);
            }
                
            ConfirmationNotification("Deleted selected messages successfully");
            return RedirectToAction("index", new { OrganisationId = model.OrgId, Service = model.Svc });
		}

        [HttpPost, ExportViewData]
        public ActionResult DeleteAll(MessageFailuresActionPostModel model)
        {
            Core.Session.MasterRaven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(new MessageEnvelopes().IndexName, new IndexQuery
            {
                Query = "Id:*"
            }, allowStale: true);

            ConfirmationNotification("Deleted all messages successfully");
            return RedirectToAction("index", new { OrganisationId = model.OrgId, Service = model.Svc });
        }

        [HttpPost, ExportViewData]
        public ActionResult Retry(MessageFailuresActionPostModel model)
        {
            if (model.EnvelopeIds == null || !model.EnvelopeIds.Any())
            {
                ErrorNotification("Please select messages to retry");
                return RedirectToAction("index", new { OrganisationId = model.OrgId, Service = model.Svc });
            }

            foreach (var envelopeId in model.EnvelopeIds)
            {
                var envelope = Core.Session.MasterRaven.Load<MessageEnvelope>(MessageEnvelope.GetId(envelopeId));

                if (envelope != null)
                    DoRetry(envelope);
            }

            ConfirmationNotification("Retried selected messages successfully");
            return RedirectToAction("index", new { OrganisationId = model.OrgId, Service = model.Svc });
		}

        [HttpPost, ExportViewData]
        public ActionResult RetryAll(MessageFailuresActionPostModel model)
        {
            foreach (var envelope in Core.Session.MasterRaven.Query<MessageEnvelope, MessageEnvelopes>().GetAllItemsAsList(128))
            {
                if (envelope != null)
                {
                    DoRetry(envelope);
                    Core.Session.MasterRaven.Delete(envelope);
                }
            }

            ConfirmationNotification("Retried all messages successfully");
            return RedirectToAction("index", new { OrganisationId = model.OrgId, Service = model.Svc });
        }

        private void DoRetry(MessageEnvelope envelope)
        {
            var e = new MessageEnvelope
            {
                Message = envelope.Message,
                MessageType = envelope.MessageType,
                OrganisationId = envelope.OrganisationId,
                QueueUrl = envelope.QueueUrl,
                GeneratedOnUtc = DateTime.UtcNow
            };

            _amazonSQS.SendMessage(new SendMessageRequest
            {
                QueueUrl = envelope.QueueUrl,
                MessageBody = JsonConvert.SerializeObject(e),
            });
        }
    }
}
