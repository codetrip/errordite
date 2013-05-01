using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Errordite.Core.Exceptions;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.Exceptions;
using Errordite.Core.Notifications.Naming;

namespace Errordite.Core.Notifications.Queries
{
    public class GetEmailInfoQuery : IGetEmailInfoQuery
    {
        private readonly IEmailNamingMapper _mapper;

        public GetEmailInfoQuery(IEmailNamingMapper mapper)
        {
            _mapper = mapper;
        }

        public GetEmailInfoQueryResponse Invoke(GetEmailInfoQueryRequest request)
        {
            var t = _mapper.NameToInfo(request.EmailName);

            if (t == null)
                throw new ErrorditeEmailNotFoundException(request.EmailName);

            var ret = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p => !p.Name.IsIn("TemplateName", "Organisation") && p.CanWrite)
                .ToDictionary(p => p.Name, p => GetType(p.PropertyType));

            return new GetEmailInfoQueryResponse() { Parameters = ret, EmailInfoType = t, EmailName = _mapper.InfoToName(t)};
        }

        private static EmailParameterType GetType(Type t)
        {
			if (t == typeof(string) || t.IsEnum)
				return EmailParameterType.String;
			if (t == typeof(int) || t == typeof(int?) || t == typeof(long) || t == typeof(long?) || t == typeof(short) || t == typeof(short?))
				return EmailParameterType.Int;
			if (t == typeof(decimal) || t == typeof(decimal?))
				return EmailParameterType.Decimal;
			if (t == typeof(DateTime) || t == typeof(DateTime?))
				return EmailParameterType.DateTime;
			if (t == typeof(bool))
				return EmailParameterType.Bool;
			if (t == typeof(Guid))
				return EmailParameterType.Guid;
			if (t.GetInterface("IEnumerable", true) != null)
				return EmailParameterType.List;
            if (t == typeof(User))
                return EmailParameterType.User;

            throw new ErrorditeUnexpectedValueException("EmailParameter Type", t.FullName);
        }
    }

    public interface IGetEmailInfoQuery : IQuery<GetEmailInfoQueryRequest, GetEmailInfoQueryResponse> { }

    public class GetEmailInfoQueryResponse
    {
        public IDictionary<string, EmailParameterType> Parameters { get; set; }
        public Type EmailInfoType { get; set; }
        public string EmailName { get; set; }
    }

    public class GetEmailInfoQueryRequest
    {
        public string EmailName { get; set; }
    }
}