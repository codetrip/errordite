using System;

namespace Errordite.Samples.WebForms
{
    public partial class ForceError : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int zero = 0;
            int test = 100 / zero;
        }
    }
}
