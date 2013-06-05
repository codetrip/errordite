namespace Errordite.Web.ActionResults
{
    public class JsonResultObject
    {
        public JsonResultObject()
        {
            success = false;
            message = "";
            redirect = "";
            data = "";
            html = "";
        }

        public bool success { get; set; }
        public string message { get; set; }
        public string redirect { get; set; }
        public object data { get; set; }
        public string html { get; set; }
        public string status { get; set; }
    }
}