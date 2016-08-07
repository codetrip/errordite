using System.Collections.Generic;
using System.Linq;
using Errordite.Client;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Extensions
{
	public static class ClientErrorExtensions
	{
		public static Error GetError(this ClientError clientError, Application application)
		{
			var instance = new Error
			{
				ApplicationId = application.Id,
				TimestampUtc = clientError.TimestampUtc.ToDateTimeOffset(application.TimezoneId),
				MachineName = clientError.MachineName,
				Url = clientError.Url,
				UserAgent = clientError.UserAgent,
				Version = clientError.Version,
				OrganisationId = application.OrganisationId,
				ExceptionInfos = GetErrorInfo(clientError.ExceptionInfo).ToArray(),
				ContextData = clientError.ContextData,
				Messages = clientError.Messages == null ? null : clientError.Messages.Select(m => new TraceMessage
				{
					Message = m.Message,
					Timestamp = m.TimestampUtc
				}).ToList()
			};

			//temp thing to move context data to error for clients not updated to latest build
			if (instance.ContextData == null || instance.ContextData.Count == 0)
			{
				MoveContextData(instance);
			}

			return instance;
		}

		private static void MoveContextData(Error error)
		{
			if (error.ExceptionInfos == null || error.ExceptionInfos.Length == 0 || error.ExceptionInfos[0].ExtraData == null)
				return;

			var exceptionData = error.ExceptionInfos[0].ExtraData.Where(s => s.Key.StartsWith("Exception"));
			var contextData = error.ExceptionInfos[0].ExtraData.Where(s => !s.Key.StartsWith("Exception"));

			error.ContextData = contextData.ToDictionary(s => s.Key, s => s.Value);
			error.ExceptionInfos[0].ExtraData = exceptionData.ToDictionary(s => s.Key, s => s.Value);
		}

		private static IEnumerable<Domain.Error.ExceptionInfo> GetErrorInfo(Client.ExceptionInfo clientExceptionInfo)
		{
			var exceptionInfo = new Domain.Error.ExceptionInfo
			{
				StackTrace = clientExceptionInfo.StackTrace,
				Message = clientExceptionInfo.Message,
				Type = clientExceptionInfo.ExceptionType,
				ExtraData = clientExceptionInfo.Data == null ? null : clientExceptionInfo.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
				Module = clientExceptionInfo.Source,
				MethodName = clientExceptionInfo.MethodName
			};

			yield return exceptionInfo;

			if (clientExceptionInfo.InnerExceptionInfo != null)
				foreach (var innerExceptionInfo in GetErrorInfo(clientExceptionInfo.InnerExceptionInfo))
					yield return innerExceptionInfo;
		}
	}
}
