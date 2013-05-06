using Errordite.Core.Auditing.Entities;

namespace Errordite.Core.Auditing
{
    public interface IErrorAugmenter
    {
        void Augment(Error error);
    }
}
