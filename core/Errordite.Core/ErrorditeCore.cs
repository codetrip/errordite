
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Groups.Queries;
using Errordite.Core.Identity;
using Errordite.Core.Users.Queries;

namespace Errordite.Core
{
    public interface IErrorditeCore
    {
        Page<User> GetUsers(PageRequestWithSort paging = null, string groupId = null);
        Page<Group> GetGroups(PageRequestWithSort paging = null);
        Page<Application> GetApplications(PageRequestWithSort paging = null);
        ErrorditeConfiguration Configuration { get; set; }
        AppContext AppContext { get; set; }
        IAppSession Session { get; set; }
    }

    public class ErrorditeCore : IErrorditeCore
    {
        private readonly IGetUsersQuery _getUsersQuery;
        private readonly IGetApplicationsQuery _getApplicationsQuery;
        private readonly IGetGroupsQuery _getGroupsQuery;

        public ErrorditeCore(IGetUsersQuery getUsersQuery, IGetApplicationsQuery getApplicationsQuery, IGetGroupsQuery getGroupsQuery, ErrorditeConfiguration configuration, AppContext appContext, IAppSession session)
        {
            _getUsersQuery = getUsersQuery;
            _getApplicationsQuery = getApplicationsQuery;
            _getGroupsQuery = getGroupsQuery;
            Configuration = configuration;
            AppContext = appContext;
            Session = session;
        }

        public ErrorditeConfiguration Configuration { get; set; }
        public AppContext AppContext { get; set; }
        public IAppSession Session { get; set; }

        public Page<User> GetUsers(PageRequestWithSort paging = null, string groupId = null)
        {
            var users = _getUsersQuery.Invoke(new GetUsersRequest
            {
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                Paging = new PageRequestWithSort(1, Configuration.MaxPageSize)
            }).Users;

            if (groupId.IsNotNullOrEmpty())
                return users.Filter(u => u.GroupIds.Contains(Group.GetId(groupId)), paging);

            if (paging != null)
                return users.AdjustSetForPaging(paging);

            return users;
        }

        public Page<Application> GetApplications(PageRequestWithSort paging = null)
        {
            var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
            {
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                Paging = new PageRequestWithSort(1, Configuration.MaxPageSize)
            }).Applications;

            if (paging == null)
                return applications;

            return applications.AdjustSetForPaging(paging);
        }

        public Page<Group> GetGroups(PageRequestWithSort paging = null)
        {
            var groups = _getGroupsQuery.Invoke(new GetGroupsRequest
            {
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                Paging = new PageRequestWithSort(1, Configuration.MaxPageSize)
            }).Groups;

            if (paging == null)
                return groups;

            return groups.AdjustSetForPaging(paging);
        }
    }
}
