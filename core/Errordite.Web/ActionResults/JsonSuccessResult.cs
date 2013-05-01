using System;
using System.Web.Mvc;

namespace Errordite.Web.ActionResults
{
    public class JsonSuccessResult : JsonResult
    {
        public JsonSuccessResult(object data = null, bool allowGet = false, string message = null, Enum status = null)
        {
            Data = new JsonResultObject
            {
                success = true,
                data = data ?? string.Empty,
                redirect = string.Empty,
                message = message ?? string.Empty,
                status = status == null ? "" : status.ToString(),
            };

            if (allowGet)
                JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }
    }
}
