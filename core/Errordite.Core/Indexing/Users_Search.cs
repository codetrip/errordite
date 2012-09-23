using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Organisation;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Users_Search : AbstractIndexCreationTask<User>
    {
        public Users_Search()
        {
            Map = users => from user in users
                            select new
                            {
                                user.Id,
                                user.GroupIds,
                                user.OrganisationId,
                                user.Password,
                                user.PasswordToken,
                                user.Email,
                                user.LastName
                            };
        }
    }
}