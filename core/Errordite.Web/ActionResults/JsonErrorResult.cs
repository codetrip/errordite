using System.Web.Mvc;

namespace Errordite.Web.ActionResults
{
    public class JsonErrorResult : JsonResult
    {
        public JsonErrorResult(string errorMessage = null, string redirect = null, bool allowGet = false)
        {
            Data = new JsonResultObject
            {
                success = false,
                message = errorMessage ?? string.Empty,
                redirect = redirect ?? string.Empty,
                data = string.Empty,
            };

            if(allowGet)
                JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }
    }
}
