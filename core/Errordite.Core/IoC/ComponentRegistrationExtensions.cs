using Castle.MicroKernel.Registration;

namespace Errordite.Core.IoC
{
	public static class ComponentRegistrationExtensions
	{
	    public static ComponentRegistration<T> AssignPerWebRequestOrPerThread<T>(this ComponentRegistration<T> registration, bool registerPerWebRequest) 
            where T : class
	    {
	        return registerPerWebRequest
	            ? registration.LifeStyle.PerWebRequest
	            : registration.LifeStyle.PerThread;
	    }
	}
}