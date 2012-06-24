using System.Web.Mvc;

namespace Errordite.Web.ActionResults
{
    public class JsonSuccessHtmlResult : JsonResult
    {
        public JsonSuccessHtmlResult(string html)
        {
            Data = new JsonResultObject
            {
                success = true,
                html = html
            };
        }
    }
}