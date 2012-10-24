using CodeTrip.Core.Auditing.Entities;

namespace CodeTrip.Core.Auditing
{
    public interface IErrorAugmenter
    {
        void Augment(Error error);
    }
}
