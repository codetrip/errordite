using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using Errordite.Client;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Organisation;
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
			string url;
			var clientError = new ClientError
				{
					TimestampUtc = DateTime.UtcNow,
					Token = token,
					MachineName = "TEST-MACHINE-1",
					UserAgent =
						"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.110 Safari/537.36",
					Version = "1.0.0.0",
					ContextData = new ErrorData
					{
						{"Request.HttpMethod", "GET"},
						{"Request.Header.Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
						{"Request.Header.Accept-Encoding", "gzip,deflate,sdch"},
					},
					ExceptionInfo = GetExceptionInfoFromErrorTemplate(errorId, out url),
					Messages = new List<LogMessage>
					{
						new LogMessage
						{
							Message = "This is an example of a log message captured through our Errordite log4net module",
							TimestampUtc = DateTime.UtcNow
						},
					},
					Url = url
				};

			return clientError;
		}

		[HttpPost, ExportViewData]
		public ActionResult GenerateError(GenerateErrorPostModel model)
		{
			var clientError = JsonConvert.DeserializeObject<ClientError>(model.Json);

			var response = _getApplicationByTokenQuery.Invoke(new GetApplicationByTokenRequest
            {
				Token = clientError.Token,
                CurrentUser = Errordite.Core.Domain.Organisation.User.System()
            });

			if (response.Status != ApplicationStatus.Ok)
			{
				ErrorNotification("Failed to locate application from token, please check the token and try again");
				return RedirectToAction("index");
			}

			var error = clientError.GetError(response.Application);
			var success = false;
			ReceiveErrorResponse receiveErrorResponse = null;

			//try and generate the error 3 times, to catch any concurrency errors if lots of users are generating test errors at the same time
			for (int i = 0; i < 3; i++)
			{
				try
				{
					receiveErrorResponse = Generate(error, response.Application);
					success = true;
					break;
				}
				catch{}
			}

			if (success)
			{
				ConfirmationNotification(new MvcHtmlString("Test error generated - attached to issue <a href='{0}'>{1}</a>".FormatWith(
						Url.Issue(IdHelper.GetFriendlyId(receiveErrorResponse.IssueId)),
						IdHelper.GetFriendlyId(receiveErrorResponse.IssueId))));
			}
			else
			{
				ErrorNotification("Failed to generate error, please try again.");
			}
			
			return RedirectToAction("index");
		}

		private ReceiveErrorResponse Generate(Core.Domain.Error.Error error, Application application)
		{
			var task = Core.Session.ReceiveHttpClient.PostJsonAsync("error", new ReceiveErrorRequest
			{
				Error = error,
				ApplicationId = application.Id,
				Organisation = AppContext.CurrentUser.ActiveOrganisation,
			}).ContinueWith(t =>
			{
				t.Result.EnsureSuccessStatusCode();
				return t.Result.Content.ReadAsAsync<ReceiveErrorResponse>().Result;
			});

			return task.Result;
		}

		private ExceptionInfo GetExceptionInfoFromErrorTemplate(string errorTemplateId, out string url)
		{
			switch (errorTemplateId)
			{
				case "1":
					{
						url = "http://www.errordite.com/test/error/argumentnullexception";
						return new ExceptionInfo
						{
							MethodName = "Errordite.Demo.GenerateArgumentNullException",
							Message = "Value cannot be null. Parameter name: index",
							ExceptionType = "System.ArgumentNullException",
							Source = "Errordite.Test",
							StackTrace = @"at Errordite.Test.Module.GetFacets()
at Errordite.Test.Module.ProcessRequest(HttpContext context)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)"
						};
					}
				case "2":
					{
						url = "http://www.errordite.com/test/error/nullreferenceexception";
						return new ExceptionInfo
						{
							MethodName = "Errordite.Demo.GenerateNullReferenceException",
							Message = "Object reference not set to an instance of an object.",
							ExceptionType = "System.NullReferenceException",
							Source = "Errordite.TestNullRef",
							StackTrace = @"at Errordite.Test.Module.GetObject()
at Errordite.Test.Module.ProcessRequest(HttpContext context)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)"
						};
					}
				case "3":
					{
						url = "http://www.errordite.com/test/error/dividebyzero";
						return new ExceptionInfo
						{
							MethodName = "Errordite.Demo.GenerateDivideByZeroException",
							Message = "Attempted to divide by zero.",
							ExceptionType = "System.DivideByZeroException",
							Source = "Errordite.TestDivideByZero",
							StackTrace = @"at Errordite.Test.Module.DivideByZero()
at Errordite.Test.Module.ProcessRequest(HttpContext context)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)"
						};
					}
				default:
					{
						url = "http://www.errordite.com/test/error/invalidoperationexception";
						return new ExceptionInfo
						{
							MethodName = "Errordite.Demo.GenerateInvalidOperation",
							Message = "Attempt to do womething which is invalid",
							ExceptionType = "System.InvalidOperationException",
							Source = "Errordite.TestInvalidOperation",
							StackTrace = @"at Errordite.Test.Module.InvalidOperation()
at Errordite.Test.Module.ProcessRequest(HttpContext context)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)"
						};
					}
			}
		}
    }
}
