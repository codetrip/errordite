using Castle.Windsor;

namespace CodeTrip.Core.IoC
{
    public interface IContainerInitialiser
    {
        void Init(IWindsorContainer container);
    }
}