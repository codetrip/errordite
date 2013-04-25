using System.Web.Http.Filters;
using System.Web.Mvc;
using Errordite.Core.IoC;

namespace Errordite.Core.Session
{
    public class SessionActionFilterAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //don't commit if there has been an error
            if (filterContext.Exception != null)
                return;

            var session = ObjectFactory.GetObject<IAppSession>();

            try
            {
                session.Commit();
            }
            finally
            {
                session.Close();
            }
        }
    }

    public class SessionActionFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //don't commit if there has been an error
            if (actionExecutedContext.Exception != null)
                return;

            var session = ObjectFactory.GetObject<IAppSession>();

            try
            {
                session.Commit();
            }
            finally
            {
                session.Close();
            }
        }
    }
}