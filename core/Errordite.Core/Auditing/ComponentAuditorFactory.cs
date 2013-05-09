using Errordite.Core.Auditing.Auditors;
using Errordite.Core.Auditing.Entities;

namespace Errordite.Core.Auditing
{
    public interface IComponentAuditorFactory
    {
        IComponentAuditor Create(string loggerName);
    }

    public class ComponentAuditorFactory : IComponentAuditorFactory
    {
        public IComponentAuditor Create(string loggerName)
        {
            return new Log4NetComponentAuditor(loggerName);
        }
    }
}
