using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Raven.Client.Linq;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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

            Statistics stats = new Statistics();

            var results = Session.Raven.Query<IssueDocument, Issues_Search>()
                .Where(r => r.OrganisationId == Organisation.GetId(request.OrganisationId))
                .ToFacets(CoreConstants.FacetDocuments.IssueStatus);

            var statusFacetValues = results["Status"];
            var statsType = stats.GetType();

            foreach(string status in Enum.GetNames(typeof(IssueStatus)))
            {
                var facetValue = statusFacetValues.FirstOrDefault(f => f.Range.Equals(status, StringComparison.OrdinalIgnoreCase));

                if(facetValue != null)
                {
                    var propertyInfo = statsType.GetProperty(status);
                    if(propertyInfo != null)
                        propertyInfo.SetValue(stats, facetValue.Count, null);
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
        public string OrganisationId { get; set; }
    }
}
