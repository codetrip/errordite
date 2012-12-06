
using System;
using System.Text;
using Castle.Core;
using CodeTrip.Core.IoC;
using Errordite.Core.Domain.Organisation;
using Errordite.Test.Automation.Configuration;
using Errordite.Test.Automation.IoC;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Errordite.Test.Automation
{
    [TestFixture]
    public class AutomatedTestBase
    {
        protected IWebDriver SamplesDriver { get; set; }
        protected Armoury Armoury { get; private set; }
        protected StringBuilder VerificationErrors { get; set; }
        protected User LoggedInUser { get; set; }
        protected AutomationConfiguration Configuration;

        #region Setup & TearDown

        protected T Get<T>()
        {
            return ObjectFactory.GetObject<T>();
        }

        
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup");
            Armoury = Get<Armoury>();
            VerificationErrors = new StringBuilder();
            Configuration = Get<AutomationConfiguration>();
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("TearDown");

            try
            {
                Armoury.ErrorditeDriver.Quit();
            }
            catch
            {}

            try
            {
                if (SamplesDriver != null)
                    SamplesDriver.Quit();
            }
            catch
            {  }

            //perform the deletes
            foreach(var action in Armoury.AutomationSession.DeleteActions)
            {
                Console.WriteLine("Performing Delete Action");
                //action();
            }

            Armoury.RavenDriver.SaveChanges();
            Assert.AreEqual(string.Empty, VerificationErrors.ToString());
        }

        [TestFixtureSetUp]
        public void ErrorditeTestBaseFixtureSetUp()
        {
            ObjectFactory.Container.Kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
            ObjectFactory.Container.Install(new AutomationInstaller());
        }

        void Kernel_ComponentModelCreated(ComponentModel model)
        {
            if (model.LifestyleType == LifestyleType.PerWebRequest)
                model.LifestyleType = LifestyleType.Thread;
        }

        #endregion

    }
}
