using System;
using System.Collections.Generic;

namespace Errordite.Test.Automation
{
    public class AutomationSession
    {
        public AutomationSession()
        {
            DeleteActions = new List<Action>();
        }

        public List<Action> DeleteActions { get; private set; }
    }
}