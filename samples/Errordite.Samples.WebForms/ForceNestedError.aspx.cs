using System;

namespace Errordite.Samples.WebForms
{
    public partial class ForceNestedError : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int zero = 0;
                int test = 100 / zero;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Something went wrong!", ex);
            }
        }
    }
}
