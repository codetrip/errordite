using System;
using System.Linq;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Raven.Client;
using Errordite.Core.Extensions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Organisations.Queries
{
    public class GetOrganisationStatisticsQuery : SessionAccessBase, IGetOrganisationStatisticsQuery
    {
        private readonly IErrorditeCore _errorditeCore;

        public GetOrganisationStatisticsQuery(IErrorditeCore errorditeCore)
        {
            _errorditeCore = errorditeCore;
        }

        public GetOrganisationStatisticsResponse Invoke(GetOrganisationStatisticsRequest request)
        {
            Trace("Starting...");

            var stats = new Statistics();

            var results = Session.Raven.Query<IssueDocument, Issues_Search>()
                .ConditionalWhere(r => r.ApplicationId == Application.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .ToFacets(CoreConstants.FacetDocuments.IssueStatus);
            
            var statusFacetValues = results.Results["Status"];
            var statsType = stats.GetType();

            foreach(string status in Enum.GetNames(typeof(IssueStatus)))
            {
                var facetValue = statusFacetValues.Values.FirstOrDefault(f => f.Range.Equals(status, StringComparison.OrdinalIgnoreCase));

                if(facetValue != null)
                {
                    var propertyInfo = statsType.GetProperty(status);
                    if(propertyInfo != null)
                        propertyInfo.SetValue(stats, facetValue.Hits, null);
                }
            }

            stats.Issues = stats.TotalIssues;
            stats.Users = _errorditeCore.GetUsers().PagingStatus.TotalItems;
            stats.Applications = _errorditeCore.GetApplications().PagingStatus.TotalItems;
            stats.Groups = _errorditeCore.GetGroups().PagingStatus.TotalItems;

            return new GetOrganisationStatisticsResponse
            {
                Statistics = stats
            };
        }
    }

    public interface IGetOrganisationStatisticsQuery : IQuery<GetOrganisationStatisticsRequest, GetOrganisationStatisticsResponse>
    { }

    public class GetOrganisationStatisticsResponse  
    {
        public Statistics Statistics { get; set; }
    }

    public class GetOrganisationStatisticsRequest
    {
        public string ApplicationId { get; set; }
    }
}
