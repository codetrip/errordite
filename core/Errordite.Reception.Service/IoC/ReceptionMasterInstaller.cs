﻿using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Reception.Service.IoC
{
    public class ReceptionMasterInstaller : MasterInstallerBase
    {
        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new CoreInstaller("Errordite.Reception"),
                    new ErrorditeCoreInstaller(),
                    new ReceptionNServiceBusInstaller(), 
                    new PerThreadAppSessionInstaller(),
                    new ReceptionServiceInstaller(), 
                };
            }
        }
    }
}