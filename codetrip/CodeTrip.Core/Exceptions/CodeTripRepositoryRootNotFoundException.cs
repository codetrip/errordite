using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Exceptions
{
    public class CodeTripRepositoryRootNotFoundException : CodeTripException
    {
        public CodeTripRepositoryRootNotFoundException(string baseDirectory)
            : base("{0} is not in a repository.".FormatWith(baseDirectory))
        {

        }
    }
}