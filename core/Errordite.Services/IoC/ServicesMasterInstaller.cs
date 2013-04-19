using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Errordite.Core.IoC;
using Errordite.Services.Configuration;
using Errordite.Core.Extensions;

namespace Errordite.Services.IoC
{
    public class ServicesMasterInstaller : MasterInstallerBase
    {
        private readonly ServiceInstance _instance;

        public ServicesMasterInstaller(ServiceInstance instance)
        {
            _instance = instance;
        }

        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new CoreInstaller("Errordite.{0}".FormatWith(_instance)),
                    new ErrorditeCoreInstaller(),
                    new ScopedAppSessionInstaller(), 
                    new ServicesInstaller(),
                };
            }
        }
    }
}