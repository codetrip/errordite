using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Errordite.Core.IoC;

namespace Errordite.Tasks.IoC
{
    public class TasksMasterInstaller : MasterInstallerBase
    {
        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new ErrorditeCoreInstaller("Errordite.Tasks"),
                    new TransientAppSessionInstaller(), 
                    new TasksInstaller(),
                };
            }
        }
    }
}