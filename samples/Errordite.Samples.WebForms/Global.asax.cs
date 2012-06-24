using System;
using System.Diagnostics;
using Errordite.Client;

namespace Errordite.Samples.WebForms
{
    public class Global : System.Web.HttpApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            ErrorditeClient.SetErrorNotificationAction(exception => Trace.Write(exception.ToString()));
        }


        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            ErrorditeClient.ReportException(Server.GetLastError());
        }

        void Session_Start(object sender, EventArgs e)
        {
            
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
