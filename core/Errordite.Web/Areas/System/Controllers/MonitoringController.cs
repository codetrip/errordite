using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Monitoring.Queries;
using Errordite.Core.Paging;
using Errordite.Web.Areas.System.Models.Monitoring;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.Areas.System.Controllers
{
    public class MonitoringController : ErrorditeController
    {
	    private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
	    private readonly IGetMessageEnvelopes _getMessageEnvelopes;

	    public MonitoringController(IGetMessageEnvelopes getMessageEnvelopes, IPagingViewModelGenerator pagingViewModelGenerator)
	    {
		    _getMessageEnvelopes = getMessageEnvelopes;
		    _pagingViewModelGenerator = pagingViewModelGenerator;
	    }

	    [PagingView]
	    public ActionResult Index(ServiceFailuresPostModel model)
        {
			var pagingRequest = GetSinglePagingRequest();
			var failures = _getMessageEnvelopes.Invoke(new GetMessageEnvelopesRequest
			{
				OrganisationId = model.OrganisationId,
				Service = model.Service,
				Paging = pagingRequest
			});

		    var viewModel = new ServiceFailuresViewModel
			{
				Envelopes = failures.Envelopes.Items,
				OrganisationId = model.OrganisationId,
				Service = model.Service,
				Services = Service.Events.EnumToSelectList("Service"),
				Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, failures.Envelopes.PagingStatus, pagingRequest)
			};

			return View(viewModel);
        }

		public ActionResult DeleteMessages()
		{
			return new EmptyResult();
		}

		public ActionResult RetryMessages()
		{
			return new EmptyResult();
		}
    }
}
