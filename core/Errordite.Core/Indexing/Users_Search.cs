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

            Analyzers = new Dictionary<Expression<Func<User, object>>, string>
            {
                { e => e.Id, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.GroupIds, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.OrganisationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Password, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.PasswordToken, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Email, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.LastName, typeof(KeywordAnalyzer).AssemblyQualifiedName },
            };

            Stores = new Dictionary<Expression<Func<User, object>>, FieldStorage>
            {
                {e => e.Id, FieldStorage.No},
                {e => e.GroupIds, FieldStorage.No},
                {e => e.OrganisationId, FieldStorage.No},
                {e => e.Password, FieldStorage.No},
                {e => e.PasswordToken, FieldStorage.No},
                {e => e.Email, FieldStorage.No},
                {e => e.LastName, FieldStorage.No}
            };

            Sort(e => e.LastName, SortOptions.String);
        }
    }
}