//using System.Linq;
//using Errordite.Core.Domain.Error;
//using Raven.Client.Indexes;

//namespace Errordite.Core.Indexing
//{
//    public class UnloggedErrors : AbstractIndexCreationTask<UnloggedError>
//    {
//        public UnloggedErrors()
//        {
//            Map = errors => from doc in errors
//                            select new
//                                {
//                                    doc.TimestampUtc,
//                                    doc.IssueId,
//                                    doc.ApplicationId
//                                };
//        }
//    }
//}