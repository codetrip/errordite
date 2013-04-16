using CodeTrip.Core.Auditing.Auditors;
using CodeTrip.Core.Auditing.Entities;

namespace CodeTrip.Core.Auditing
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
