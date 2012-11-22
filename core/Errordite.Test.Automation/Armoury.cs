using Errordite.Core.Session;
using Errordite.Test.Automation.Drivers;
using Errordite.Test.Automation.Drivers.ErrorditeDriver;

namespace Errordite.Test.Automation
{
    public class Armoury
    {
        public RavenDriver RavenDriver { get; set; }
        public IOrganisationDriver OrganisationDriver { get; set; }
        public ErrorditeDriver ErrorditeDriver { get; set; }
        public ErrorditeClientDriver ErrorditeClientDriver { get; set; }
        public AutomationSession AutomationSession { get; private set; }

        public IAppSession AppSession { get; set; }

        public Armoury(RavenDriver ravenDriver, IOrganisationDriver organisationDriver, ErrorditeDriver errorditeDriver, AutomationSession automationSession)
        {
            AutomationSession = automationSession;
            RavenDriver = ravenDriver;
            OrganisationDriver = organisationDriver;
            ErrorditeDriver = errorditeDriver;
        }
    }
}
