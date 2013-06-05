using System;
using System.Web.Mvc;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Exceptions;

namespace Errordite.Core.Auditing
{
    public class AuditingController : Controller, ISupportsAuditing
    {
        private IComponentAuditor _auditor;

        public IComponentAuditor Auditor
        {
            get { return _auditor; }
            set
            {
                _auditor = value;
                _auditor.ModuleName = GetType().Name;
            }
        }

        protected void AssertAuditor()
        {
            if (Auditor == null)
                throw new ErrorditeDependentComponentException<IComponentAuditor>();
        }

        public void Trace(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Trace(GetType(), message, args);
        }

        public void Info(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Info(GetType(), message, args);
        }

        public void Warning(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Warning(GetType(), message, args);
        }

        public void Error(Exception e, int eventId)
        {
            AssertAuditor();
            Auditor.Error(GetType(), eventId, e);
        }

        public void Error(Exception e)
        {
            Error(e, 0);
        }

        public void Error(string message, params object[] args)
        {
            Error(message, 0, args);
        }

        public void Error(string message, int eventId, params object[] args)
        {
            AssertAuditor();
            Auditor.Error(GetType(), eventId, message, args);
        }
    }
}