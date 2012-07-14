using System.Web.Mvc;
using CodeTrip.Core.IoC;

namespace Errordite.Core.Session
{
    public class SessionActionFilterAttribute : ActionFilterAttribute
    {
        //private const string TransactionScopeItemId = "__TransactionScope";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //TODO: transactionscope not working - do we need it?
            //var ts = new TransactionScope();
            //filterContext.HttpContext.Items[TransactionScopeItemId] = ts;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //if (filterContext.Exception != null)
            //{
            //    var ts = (TransactionScope)filterContext.HttpContext.Items[TransactionScopeItemId];
            //    if (ts != null)
            //        ts.Dispose();
            //}
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var session = ObjectFactory.GetObject<IAppSession>();

            try
            {
                session.Commit();
            }
            finally
            {
                session.Close();
            }

            //var ts = (TransactionScope)filterContext.HttpContext.Items[TransactionScopeItemId];
            //if (ts != null)
            //{
            //    if (filterContext.Exception == null)
            //        ts.Complete();
            //    else 
            //        ts.Dispose();
            //}
        }
    }
}