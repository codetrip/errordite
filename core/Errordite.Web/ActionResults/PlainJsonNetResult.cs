using System.Web.Mvc;

namespace Errordite.Web.ActionResults
{
    public class PlainJsonNetResult : JsonNetResult
    {
        public PlainJsonNetResult(object data, bool allowGet = false)
        {
            Data = data;
            if (allowGet)
                JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }
    }
}