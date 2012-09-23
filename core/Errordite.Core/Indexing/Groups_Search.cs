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
    public class Groups_Search : AbstractIndexCreationTask<Group>
    {
        public Groups_Search()
        {
            Map = groups => from g in groups
                            select new
                            {
                                g.Id,
                                g.OrganisationId,
                                g.Name
                            };
        }
    }
}