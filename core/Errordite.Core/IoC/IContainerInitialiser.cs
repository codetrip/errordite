using Castle.Windsor;

namespace Errordite.Core.IoC
{
    public interface IContainerInitialiser
    {
        void Init(IWindsorContainer container);
    }
}