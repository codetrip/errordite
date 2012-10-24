using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Auditing;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class ErrorditeErrors : AbstractIndexCreationTask<ErrorditeError>
    {
        public ErrorditeErrors()
        {
            Map = errors => from doc in errors select new
            {
                doc.Type,
                doc.TimestampUtc,
                doc.Application,
                doc.Text,
                doc.MessageId
            };

            Indexes = new Dictionary<Expression<Func<ErrorditeError, object>>, FieldIndexing>
            {
                {e => e.Text, FieldIndexing.Analyzed},
            };
        }
    }
}