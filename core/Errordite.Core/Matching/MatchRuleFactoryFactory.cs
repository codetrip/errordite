﻿using System.Collections.Generic;
using Errordite.Core.Extensions;
using Errordite.Core.IoC;

namespace Errordite.Core.Matching
{
    public interface IMatchRuleFactoryFactory
    {
        IMatchRuleFactory Create(string id);
        IEnumerable<IMatchRuleFactory> Create();
    }

    public class MatchRuleFactoryFactory : IMatchRuleFactoryFactory
    {
        public IMatchRuleFactory Create(string id)
        {
            return ObjectFactory.GetObject<IMatchRuleFactory>(CoreConstants.MatchRuleFactoryIdFormat.FormatWith(id));
        }

        public IEnumerable<IMatchRuleFactory> Create()
        {
            return ObjectFactory.Container.ResolveAll<IMatchRuleFactory>();
        }
    }
}
