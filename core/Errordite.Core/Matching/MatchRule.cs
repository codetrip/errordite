using Errordite.Core.Domain.Error;
using ProtoBuf;

namespace Errordite.Core.Matching
{
    [ProtoContract, ProtoInclude(100, typeof(PropertyMatchRule))]
    public interface IMatchRule
    {
        bool IsMatch(Error error);
        string GetDescription();
    }

    [ProtoContract]
    public abstract class MatchRule : IMatchRule
    {
        public abstract bool IsMatch(Error error);
        public abstract string GetDescription();
    }
}