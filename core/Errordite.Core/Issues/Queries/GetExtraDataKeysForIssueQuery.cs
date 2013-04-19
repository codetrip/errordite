using System.Collections.Generic;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using System.Linq;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Queries
{
    public class GetExtraDataKeysForIssueQuery : SessionAccessBase, IGetExtraDataKeysForIssueQuery
    {
        public GetExtraDataKeysForIssueResponse Invoke(GetExtraDataKeysForIssueRequest request)
        {
            return new GetExtraDataKeysForIssueResponse
                {
                    Keys = Query<IssueExtraDataKeys, Issues_ExtraDataKeys>()
                            .FirstOrDefault(i => i.IssueId == Issue.GetId(request.IssueId))
                            .IfPoss(r => r.Keys ?? new List<string>(), new List<string>()),
                };
        }
    }

    public interface IGetExtraDataKeysForIssueQuery : IQuery<GetExtraDataKeysForIssueRequest, GetExtraDataKeysForIssueResponse>
    {}

    public class GetExtraDataKeysForIssueResponse
    {
        public List<string> Keys { get; set; }
    }

    public class GetExtraDataKeysForIssueRequest
    {
        public string IssueId { get; set; }
    }
}