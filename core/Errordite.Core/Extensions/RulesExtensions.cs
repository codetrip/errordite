using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Matching;
using Errordite.Core.Extensions;

namespace Errordite.Core.Extensions
{
    public static class RulesExtensions
    {
        public static string GetHash(this IEnumerable<IMatchRule> rules)
        {
            return rules != null ? rules.Aggregate(string.Empty, (current, t) => current + (t.GetDescription().ToLowerInvariant())).Hash() : null;
        }
    }
}
