using System;
using System.Net;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Client;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Messaging;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Receive.Commands
{
    public class ProcessIncomingExceptionCommand : SessionAccessBase, IProcessIncomingExceptionCommand
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;
        private readonly IGetApplicationByTokenQuery _getApplicationByToken;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IExceptionRateLimiter _exceptionRateLimiter;
        private readonly IMessageSender _sender;

        public ProcessIncomingExceptionCommand(IGetApplicationByTokenQuery getApplicationByToken, 
            ErrorditeConfiguration configuration, 
            IReceiveErrorCommand receiveErrorCommand,
            IExceptionRateLimiter exceptionRateLimiter, 
            IMessageSender sender)
        {
            _getApplicationByToken = getApplicationByToken;
            _configuration = configuration;
            _receiveErrorCommand = receiveErrorCommand;
            _exceptionRateLimiter = exceptionRateLimiter;
            _sender = sender;
        }

        public ProcessIncomingExceptionResponse Invoke(ProcessIncomingExceptionRequest request)
        {
            TraceObject(request.Error);

            Application application;
            Organisation organisation;
            var status = TryGetApplication(request.Error.Token, out application, out organisation);
            string applicationId = null;
            string organisationId = null;

            switch(status)
            {
                case ApplicationStatus.Inactive:
                    return new ProcessIncomingExceptionResponse
                    {
                        ResponseMessage = "The application specified in the token is not currently active",
                        ResponseCode = HttpStatusCode.NotAcceptable,
                    };
                case ApplicationStatus.NotFound:
                    return new ProcessIncomingExceptionResponse
                    {
                        ResponseMessage = "The application specified in the token could not be found",
                        ResponseCode = HttpStatusCode.Unauthorized,
                    };
                case ApplicationStatus.Error:
                    return new ProcessIncomingExceptionResponse
                    {
                        ResponseCode = HttpStatusCode.InternalServerError,
                        ResponseMessage = "An unhandled error occured while attempting to store this error"
                    };
                case ApplicationStatus.InvalidOrganisation:
                    return new ProcessIncomingExceptionResponse
                    {
                        ResponseMessage = "Failed to locate the organisation specified in your token",
                        ResponseCode = HttpStatusCode.Unauthorized,
                    };
                case ApplicationStatus.InvalidToken:
                    return new ProcessIncomingExceptionResponse
                    {
                        ResponseMessage = "The token supplied is invalid, please check your token in the applications page in Errordite",
                        ResponseCode = HttpStatusCode.BadRequest,
                    };
                case ApplicationStatus.Ok:
                    {
                        applicationId = application.Id;
                        organisationId = application.OrganisationId;
                    }
                    break;
            }

            RateLimiterRule failedRule;
            if ((failedRule = _exceptionRateLimiter.Accept(applicationId)) != null)
            {
                Trace("Failed rate limiter rule named {0}", failedRule.Name);
                return new ProcessIncomingExceptionResponse
                {
                    ResponseMessage = "The error was not stored due to limits on the number of errors we can receive for you in a given time frame",
                    SpecialResponseCode = 429, //too many requests http://tools.ietf.org/html/draft-nottingham-http-new-status-02
                };
            }

			var error = request.Error.GetError(application);

            if (_configuration.ServiceBusEnabled)
            {
                _sender.Send(new ReceiveErrorMessage
                {
                    Error = error,
                    ApplicationId = applicationId,
                    OrganisationId = organisationId,
                    Token = request.Error.Token,
                },
                _configuration.GetReceiveQueueAddress(organisation.FriendlyId));
                Session.AddCommitAction(new PollNowCommitAction(organisation));
            }
            else
            {
                Trace("ServiceBus is disabled, invoking IReceiveErrorCommand");

                _receiveErrorCommand.Invoke(new ReceiveErrorRequest
                {
                    Error = error,
                    ApplicationId = applicationId,
                    Organisation = organisation,
                    Token = request.Error.Token
                });
            }

            return new ProcessIncomingExceptionResponse();
        }

        private ApplicationStatus TryGetApplication(string token, out Application application, out Organisation organisation)
        {
            try
            {
                var response = _getApplicationByToken.Invoke(new GetApplicationByTokenRequest
                {
                    Token = token,
                    CurrentUser = User.System()
                });

                application = response.Application;
                organisation = response.Organisation;

                if(application != null && !application.IsActive)
                    return ApplicationStatus.Inactive;

                return response.Status;
            }
            catch (Exception e)
            {
                Error(e);
                application = null;
                organisation = null;
                return ApplicationStatus.Error;
            }
        }
    }

    public enum ApplicationStatus
    {
        Ok,
        NotFound,
        Inactive,
        Error,
        InvalidToken,
        InvalidOrganisation
    }

    public interface IProcessIncomingExceptionCommand : ICommand<ProcessIncomingExceptionRequest, ProcessIncomingExceptionResponse>
    {}

    public class ProcessIncomingExceptionRequest
    {
        public ClientError Error { get; set; }
    }

    public class ProcessIncomingExceptionResponse
    {
        public ProcessIncomingExceptionResponse()
        {
            ResponseCode = HttpStatusCode.Accepted;
        }

        public string ResponseMessage { get; set; }

        public HttpStatusCode? ResponseCode { get; set; }
        //if the Response Code we want to use isn't in the .net enum, set it here instead
        public int? SpecialResponseCode { get; set; }

    }
}
