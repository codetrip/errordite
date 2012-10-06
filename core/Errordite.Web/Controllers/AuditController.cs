
using System;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Auditing.Queries;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.ActionFilters;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Audit;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class AuditController : ErrorditeController
    {
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IGetAuditRecordsQuery _getAuditRecordsQuery;

        public AuditController(IPagingViewModelGenerator pagingViewModelGenerator, IGetAuditRecordsQuery getAuditRecordsQuery)
        {
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _getAuditRecordsQuery = getAuditRecordsQuery;
        }

        [PagingView(DefaultSort = CoreConstants.SortFields.CompletedOnUtc, DefaultSortDescending = true), ExportViewData, ImportViewData]
        public ActionResult Index(AuditPostModel postModel)
        {
            var viewModel = new AuditViewModel();

            var pagingRequest = GetSinglePagingRequest();

            var request = new GetAuditRecordsRequest
            {
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                Paging = pagingRequest,
                Status = postModel.Status,
                AuditRecordType = postModel.AuditRecordType,
                CompletedEndDate = postModel.EndDate,
                CompletedStartDate = postModel.StartDate
            };

            var auditRecords = _getAuditRecordsQuery.Invoke(request).AuditRecords;

            viewModel.StartDate = postModel.StartDate;
            viewModel.EndDate = postModel.EndDate;
            viewModel.Status = postModel.Status;
            viewModel.AuditRecordType = postModel.AuditRecordType; 
            viewModel.AuditRecordStatuses = Enum.GetNames(typeof(AuditRecordStatus))
                .ToSelectList(u => u, u => u, u => postModel.Status.HasValue && postModel.Status.Value.ToString() == u, Resources.Audit.Status, Resources.Shared.DefaultSelectValue);
            viewModel.AuditRecordTypes = Enum.GetNames(typeof(AuditRecordType))
                .ToSelectList(u => u, u => Resources.Audit.ResourceManager.GetString(u), u => postModel.AuditRecordType.HasValue && postModel.AuditRecordType.Value.ToString() == u, Resources.Audit.Type, Resources.Shared.DefaultSelectValue);                
            viewModel.Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, auditRecords.PagingStatus, pagingRequest);
            viewModel.AuditRecords = auditRecords;

            return View(viewModel);
        }
    }
}
