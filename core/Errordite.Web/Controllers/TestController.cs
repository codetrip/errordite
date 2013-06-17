using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using Errordite.Client;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain;
using Errordite.Core.Receive.Commands;
using Errordite.Core.Web;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Models.Test;
using Errordite.Web.Extensions;
using Errordite.Core.Extensions;
using System.Linq;
using Newtonsoft.Json;

namespace Errordite.Web.Controllers
{
    public class TestController : ErrorditeController
    {
	    private readonly IGetApplicationByTokenQuery _getApplicationByTokenQuery;

	    public TestController(IGetApplicationByTokenQuery getApplicationByTokenQuery)
	    {
		    _getApplicationByTokenQuery = getApplicationByTokenQuery;
	    }

	    [ImportViewData]
        public ActionResult Index(GenerateErrorPostModel postModel)
        {
	        var model = new GenerateErrorViewModel
		    {
				Applications = Core.GetApplications().Items.ToSelectList(a => a.Token, a => a.Name, a => a.FriendlyId == postModel.Token),
				Errors = GenerateErrorViewModel.GetErrors(postModel.ErrorId),
		    };

		    model.Token = model.Applications.First().Value;
		    model.ErrorId = model.Errors.First().Value;
		    model.Json = JsonConvert.SerializeObject(GetClientError(model.ErrorId, model.Token), Formatting.Indented);

			return View(model);
        }

		public ActionResult GetJson(string errorId, string token)
		{
			return Content(JsonConvert.SerializeObject(GetClientError(errorId, token), Formatting.Indented));
		}

		public ClientError GetClientError(string errorId, string token)
		{
			var clientError = new ClientError
			{
				TimestampUtc = DateTime.UtcNow,
				Url = "http://www.errordite.com/test/error/" + errorId,
				Token = token,
				MachineName = "TEST-MACHINE-1",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.110 Safari/537.36",
				Version = "1.0.0.0",
				ContextData = new ErrorData
				{
					{"Request.HttpMethod", "GET"},
					{"Request.Header.Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
					{"Request.Header.Accept-Encoding", "gzip,deflate,sdch"},
				},
				ExceptionInfo = GetExceptionInfoFromErrorTemplate(errorId),
				Messages = new List<LogMessage>
				{
					new LogMessage { Message = "This is an example of a log message captured through our Errordite log4net module", TimestampUtc = DateTime.UtcNow },
				}
			};

			return clientError;
		}

		[HttpPost, ExportViewData]
		public ActionResult GenerateError(GenerateErrorPostModel model)
		{
			var error = JsonConvert.DeserializeObject<ClientError>(model.Json);

			var response = _getApplicationByTokenQuery.Invoke(new GetApplicationByTokenRequest
            {
				Token = error.Token,
                CurrentUser = Errordite.Core.Domain.Organisation.User.System()
            });

			if (response.Status != ApplicationStatus.Ok)
			{
				ErrorNotification("Failed to locate application from token, please check the token and try again");
				return RedirectToAction("index");
			}

			try
			{
				var task = Core.Session.ReceiveHttpClient.PostJsonAsync("error", new ReceiveErrorRequest
				{
					Error = error.GetError(response.Application),
					ApplicationId = response.Application.Id,
					Organisation = AppContext.CurrentUser.ActiveOrganisation,
				}).ContinueWith(t =>
				{
					t.Result.EnsureSuccessStatusCode();
					return t.Result.Content.ReadAsAsync<ReceiveErrorResponse>().Result;
				});

				var receiveErrorResponse = task.Result;

				ConfirmationNotification(new MvcHtmlString("Test error generated - attached to issue <a href='{0}'>{1}</a>".FormatWith(
					Url.Issue(IdHelper.GetFriendlyId(receiveErrorResponse.IssueId)), 
					IdHelper.GetFriendlyId(receiveErrorResponse.IssueId))));

				return RedirectToAction("index");
			}
			catch
			{
				ErrorNotification("Failed to generate error, please try again.");
				return RedirectToAction("index");
			}
		}

		private ExceptionInfo GetExceptionInfoFromErrorTemplate(string errorTemplateId)
		{
			switch (errorTemplateId)
			{
				default:
					{
						return new ExceptionInfo
						{
							MethodName = "Errordite.Demo.GenerateError",
							Message = "Value cannot be null. Parameter name: index",
							ExceptionType = "System.ArgumentNullException",
							Source = "Errordite.Test",
							StackTrace = @"at Errordite.Test.Module.GetFacets()
at Errordite.Test.Module.ProcessRequest(HttpContext context)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)"
						};
					}
			}
		}
    }
}
