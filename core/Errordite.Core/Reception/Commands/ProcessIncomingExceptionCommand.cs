
using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Client.Abstractions;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Messages;
using NServiceBus;
using EC = Errordite.Core.Configuration;
using ErrorditeConfiguration = Errordite.Core.Configuration.ErrorditeConfiguration;

namespace Errordite.Core.Reception.Commands
{
    public class ProcessIncomingExceptionCommand : ComponentBase, IProcessIncomingExceptionCommand
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;
        private readonly IGetApplicationByTokenQuery _getApplicationByToken;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IBus _bus;
        private readonly IExceptionRateLimiter _exceptionRateLimiter;

        public ProcessIncomingExceptionCommand(IGetApplicationByTokenQuery getApplicationByToken, 
            ErrorditeConfiguration configuration, 
            IBus bus, 
            IReceiveErrorCommand receiveErrorCommand,
            IExceptionRateLimiter exceptionRateLimiter)
        {
            _getApplicationByToken = getApplicationByToken;
            _configuration = configuration;
            _bus = bus;
            _receiveErrorCommand = receiveErrorCommand;
            _exceptionRateLimiter = exceptionRateLimiter;
        }

        public ProcessIncomingExceptionResponse Invoke(ProcessIncomingExceptionRequest request)
        {
            TraceObject(request.Error);

            Application application;
            var status = TryGetApplication(request.Error.Token, out application);
            string applicationId = null;
            string organisationId = null;

            switch(status)
            {
                case ApplicationStatus.Inactive:
                case ApplicationStatus.NotFound:
                case ApplicationStatus.Error:
                    Trace("Application not found.");
                    return new ProcessIncomingExceptionResponse();
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
                return new ProcessIncomingExceptionResponse();
            }
            var error = GetError(request.Error, application);

            if (_configuration.ServiceBusEnabled)
            {
                _bus.Send(_configuration.ReceptionQueueName, new ReceiveErrorMessage
                {
                    Error = error,
                    ApplicationId = applicationId,
                    OrganisationId = organisationId,
                    Token = request.Error.Token
                });
            }
            else
            {
                Trace("ServiceBus is disabled, invoking IReceiveErrorCommand");

                _receiveErrorCommand.Invoke(new ReceiveErrorRequest
                {
                    Error = error,
                    ApplicationId = applicationId,
                    OrganisationId = organisationId,
                    Token = request.Error.Token
                });
            }

            return new ProcessIncomingExceptionResponse();
        }

        private ApplicationStatus TryGetApplication(string token, out Application application)
        {
            try
            {
                application = _getApplicationByToken.Invoke(new GetApplicationByTokenRequest
                {
                    Token = token,
                    CurrentUser = User.System()
                }).Application;

                return application == null ? ApplicationStatus.NotFound : application.IsActive ? ApplicationStatus.Ok : ApplicationStatus.Inactive;
            }
            catch (Exception e)
            {
                Error(e);
                application = null;
                return ApplicationStatus.Error;
            }
        }

        private Error GetError(ClientError clientError, Application application)
        {
            var instance = new Error
            {
                ApplicationId = application == null ? null : application.Id,
                TimestampUtc = clientError.TimestampUtc,
                MachineName = clientError.MachineName,
                Url = GetUrl(clientError),
                UserAgent = GetUserAgent(clientError),
                Version = clientError.Version,
                Tags = clientError.Tags,
                OrganisationId = application == null ? null : application.OrganisationId,
                ExceptionInfos = GetErrorInfo(clientError.ExceptionInfo).ToArray(),
                Messages = clientError.Messages == null ? null : clientError.Messages.Select(m => new TraceMessage
                {
                    Level = m.Level,
                    Logger = m.Logger,
                    Message = m.Message,
                    Milliseconds = m.Milliseconds
                }).ToList()
            };

            return instance;
        }

        private IEnumerable<Domain.Error.ExceptionInfo> GetErrorInfo(Client.Abstractions.ExceptionInfo clientExceptionInfo)
        {
            var exceptionInfo = new Domain.Error.ExceptionInfo
            {
                StackTrace = clientExceptionInfo.StackTrace,
                Message = clientExceptionInfo.Message,
                Type = clientExceptionInfo.ExceptionType,
                //we do this because .'s in dictionary keys mean Raven querying is impossible as it is expecting a nested
                //json object rather than a property with a . in its name..  
                //the other problem is "some" (could be most, or all) non alphanumeric characters get replaced
                //with an underscore in the dynamic index name, so we may need some way of encoding keys
                ExtraData = clientExceptionInfo.Data == null ? null : clientExceptionInfo.Data.ToDictionary(kvp => kvp.Key.Replace('.', '_'), kvp => kvp.Value),
                Module = clientExceptionInfo.Source,
                MethodName = clientExceptionInfo.MethodName
            };

            yield return exceptionInfo;

            if (clientExceptionInfo.InnerExceptionInfo != null)
                foreach (var innerExceptionInfo in GetErrorInfo(clientExceptionInfo.InnerExceptionInfo))
                    yield return innerExceptionInfo;
        }

        /// <summary>
        /// NOTE: Temp until marketplace client is updated
        /// </summary>
        /// <param name="clientError"></param>
        /// <returns></returns>
        private string GetUrl(ClientError clientError)
        {
            if (clientError.Url.IsNotNullOrEmpty())
                return clientError.Url;

            if (clientError.ExceptionInfo == null || clientError.ExceptionInfo.Data == null)
                return null;

            string url;
            return clientError.ExceptionInfo.Data.TryGetValue("HttpContext.Url", out url) ? url : null;
        }

        /// <summary>
        /// NOTE: Temp until marketplace client is updated
        /// </summary>
        /// <param name="clientError"></param>
        /// <returns></returns>
        private string GetUserAgent(ClientError clientError)
        {
            if (clientError.UserAgent.IsNotNullOrEmpty())
                return clientError.UserAgent;

            string userAgent = null;

            return clientError.ExceptionInfo.IfPoss(
                e => e.Data.IfPoss(d => d.TryGetValue("HttpContext.UserAgent", out userAgent)))
                       ? userAgent
                       : null;
        }
    }

    public enum ApplicationStatus
    {
        Ok,
        NotFound,
        Inactive,
        Error
    }

    public interface IProcessIncomingExceptionCommand : ICommand<ProcessIncomingExceptionRequest, ProcessIncomingExceptionResponse>
    {}

    public class ProcessIncomingExceptionRequest
    {
        public ClientError Error { get; set; }
    }

    public class ProcessIncomingExceptionResponse
    {}
}
