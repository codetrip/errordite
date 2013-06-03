using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Errordite.Core.Configuration;
using Errordite.Core.IoC;
using Errordite.Core.Extensions;

namespace Errordite.Services.IoC
{
    public class ServicesMasterInstaller : MasterInstallerBase
    {
        private readonly Service _instance;

        public ServicesMasterInstaller(Service instance)
        {
            _instance = instance;
        }

        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new ErrorditeCoreInstaller("Errordite.{0}".FormatWith(_instance)),
                    new ScopedAppSessionInstaller(), 
                    new ServicesInstaller(),
                };
            }
        }
    }
}