using Errordite.Core.Extensions;

namespace Errordite.Core.Exceptions
{
    public class ErrorditeRepositoryRootNotFoundException : ErrorditeException
    {
        public ErrorditeRepositoryRootNotFoundException(string baseDirectory)
            : base("{0} is not in a repository.".FormatWith(baseDirectory))
        {

        }
    }
}